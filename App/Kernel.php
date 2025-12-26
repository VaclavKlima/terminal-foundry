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
    public function run(array $argv): void
    {
        $registry = $this->buildRegistry();
        $router = new Router();
        $uiJson = in_array('--ui-json', $argv, true);
        $argv = array_values(array_filter($argv, static fn (string $arg): bool => $arg !== '--ui-json'));
        $selection = $router->resolve($argv, $registry);
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
        $ui->add($selection->page()->buildElement($selection->args()));

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

        $before = get_declared_classes();
        $iterator = new \RecursiveIteratorIterator(
            new \RecursiveDirectoryIterator($appsDir, \FilesystemIterator::SKIP_DOTS)
        );

        foreach ($iterator as $file) {
            if ($file->isFile() && $file->getExtension() === 'php') {
                require_once $file->getPathname();
            }
        }

        $after = get_declared_classes();
        $newClasses = array_diff($after, $before);
        sort($newClasses);

        $apps = [];
        foreach ($newClasses as $class) {
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
            $index = (int) $trimmed - 1;
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
