<?php

namespace App;

use Framework\Cli\Runtime\App as AppAttribute;
use Framework\Cli\Runtime\AppDefinition;
use Framework\Cli\Runtime\Registry;
use Framework\Cli\Runtime\Router;
use Framework\Cli\Ui\App as UiApp;
use Framework\Cli\Ui\Renderer;

final class Kernel
{
    private ?\Framework\Cli\Runtime\RouteSelection $lastSelection = null;
    private \Framework\Cli\Runtime\FormState $state;

    public function __construct()
    {
        $this->state = new \Framework\Cli\Runtime\FormState();
    }

    public function run(array $argv): void
    {
        $registry = $this->buildRegistry();
        $router = new Router();
        $uiJson = in_array('--ui-json', $argv, true);
        $argv = array_values(array_filter($argv, static fn(string $arg): bool => $arg !== '--ui-json'));
        $selection = $this->resolveSelection($argv, $registry, $router);
        $interactive = !$uiJson && $this->isInteractive($argv);

        do {
            $options = $selection->app()->uiOptions();
            if ($uiJson) {
                $options['palette'] = [
                    'primary' => '',
                    'muted' => '',
                    'reset' => '',
                ];
            }

            $ui = new UiApp($options);
        $ui->add($selection->page()->buildElement(
            $selection->args(),
            $this->getStateAccessor($selection),
            $this->setStateAccessor($selection)
        ));

            $renderer = new Renderer();
            if ($uiJson) {
                $payload = $renderer->renderWithState($ui);
                $nodes = $renderer->renderNodes($ui);
                echo json_encode([
                        'text' => $payload['text'],
                        'actions' => $payload['actions'],
                        'nodes' => $nodes,
                        'currentApp' => $selection->app()->name(),
                    ], JSON_UNESCAPED_SLASHES) . PHP_EOL;
                return;
            }

            $rendered = $renderer->render($ui);
            echo $rendered . PHP_EOL;

            if (!$interactive) {
                return;
            }

            $choice = $this->prompt('Switch app (enter to quit): ');
            if ($choice === '') {
                return;
            }

            $selection = $this->resolveAppSelection($choice, $registry, $selection->app()->name());
        } while (true);
    }

    private function resolveSelection(array $argv, Registry $registry, Router $router): \Framework\Cli\Runtime\RouteSelection
    {
        $actionIndex = array_search('--action', $argv, true);
        if ($actionIndex !== false) {
            $actionId = $argv[$actionIndex + 1] ?? null;
            if ($actionId !== null) {
                $value = null;
                $old = null;
                $name = null;
                $valueIndex = array_search('--value', $argv, true);
                if ($valueIndex !== false) {
                    $value = $argv[$valueIndex + 1] ?? null;
                }
                $oldIndex = array_search('--old', $argv, true);
                if ($oldIndex !== false) {
                    $old = $argv[$oldIndex + 1] ?? null;
                }
                $nameIndex = array_search('--name', $argv, true);
                if ($nameIndex !== false) {
                    $name = $argv[$nameIndex + 1] ?? null;
                }

                try {
                    if ($name !== null) {
                        $this->setStateValue($registry, $router, $argv, $name, $value);
                    }
                    $result = \Framework\Cli\Runtime\ActionRegistry::invoke($actionId, [$value, $old]);
                    if ($result instanceof \Framework\Cli\Runtime\RouteIntent) {
                        $app = $registry->app($result->app());
                        $page = $app->page($result->page());
                        $selection = new \Framework\Cli\Runtime\RouteSelection($app, $page, $result->args());
                        $this->lastSelection = $selection;
                        return $selection;
                    }
                } catch (\Throwable $ex) {
                    // Ignore missing action handlers and fall back to last/default selection.
                }
            }

            if ($this->lastSelection !== null) {
                return $this->lastSelection;
            }
        }

        $selection = $router->resolve($argv, $registry);
        $this->lastSelection = $selection;
        return $selection;
    }

    private function getStateAccessor(\Framework\Cli\Runtime\RouteSelection $selection): callable
    {
        $prefix = $selection->app()->name() . '.' . $selection->page()->name() . '.';
        return function (string $key, $default = '') use ($prefix) {
            return $this->state->get($prefix, $key, $default);
        };
    }

    private function setStateAccessor(\Framework\Cli\Runtime\RouteSelection $selection): callable
    {
        $prefix = $selection->app()->name() . '.' . $selection->page()->name() . '.';
        return function (string $key, $value) use ($prefix): void {
            $this->state->set($prefix, $key, $value);
        };
    }

    private function setStateValue(Registry $registry, Router $router, array $argv, string $name, $value): void
    {
        $selection = $this->lastSelection ?? $router->resolve($argv, $registry);
        $prefix = $selection->app()->name() . '.' . $selection->page()->name() . '.';
        $this->state->set($prefix, $name, $value);
        $this->lastSelection = $selection;
    }

    private function buildRegistry(): Registry
    {
        $registry = new Registry();
        $apps = $this->discoverApps();

        $defaultAppName = null;
        foreach ($apps as $app) {
            if ($app['name'] === 'main') {
                $defaultAppName = $app['name'];
                break;
            }
        }

        foreach ($apps as $app) {
            $registry->addApp($app['definition']->build(), $app['name'] === $defaultAppName);
        }

        return $registry;
    }

    private function discoverApps(): array
    {
        $appsDir = __DIR__ . '/Apps';
        if (!is_dir($appsDir)) {
            return [];
        }

        $iterator = new \RecursiveIteratorIterator(
            new \RecursiveDirectoryIterator($appsDir, \FilesystemIterator::SKIP_DOTS)
        );

        foreach ($iterator as $file) {
            if ($file->isFile() && $file->getExtension() === 'php') {
                require_once $file->getPathname();
            }
        }

        $apps = [];
        $classes = get_declared_classes();
        sort($classes);
        foreach ($classes as $class) {
            $ref = new \ReflectionClass($class);
            if (!$ref->isInstantiable()) {
                continue;
            }

            $attributes = $ref->getAttributes(AppAttribute::class);
            if (count($attributes) === 0) {
                continue;
            }

            $instance = $ref->newInstance();
            if (!$instance instanceof AppDefinition) {
                throw new \RuntimeException("Class {$class} has #[App] but does not extend AppDefinition.");
            }

            /** @var AppAttribute $attribute */
            $attribute = $attributes[0]->newInstance();
            if ($attribute->name() !== $instance->name()) {
                throw new \RuntimeException("App name mismatch for {$class}: attribute is {$attribute->name()}.");
            }

            $apps[] = [
                'name' => $attribute->name(),
                'definition' => $instance,
            ];
        }

        return $apps;
    }

    private function resolveAppSelection(string $choice, Registry $registry, string $currentApp): \Framework\Cli\Runtime\RouteSelection
    {
        $trimmed = trim($choice);
        $apps = $registry->apps();

        if (is_numeric($trimmed)) {
            $index = (int)$trimmed - 1;
            if (isset($apps[$index])) {
                $app = $apps[$index];
                return new \Framework\Cli\Runtime\RouteSelection($app, $app->defaultPage(), []);
            }
        }

        foreach ($apps as $app) {
            if ($app->name() === $trimmed) {
                return new \Framework\Cli\Runtime\RouteSelection($app, $app->defaultPage(), []);
            }
        }

        $current = $registry->app($currentApp);
        return new \Framework\Cli\Runtime\RouteSelection($current, $current->defaultPage(), []);
    }

    private function isInteractive(array $argv): bool
    {
        if (in_array('--no-interactive', $argv, true)) {
            return false;
        }

        if (!defined('STDIN')) {
            return false;
        }

        if (function_exists('stream_isatty')) {
            return stream_isatty(STDIN);
        }

        return true;
    }

    private function prompt(string $message): string
    {
        echo $message;
        $line = fgets(STDIN);
        if ($line === false) {
            return '';
        }

        return trim($line);
    }
}
