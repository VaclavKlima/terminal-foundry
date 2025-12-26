<?php

namespace Framework\Cli\Runtime;

final class Registry
{
    private array $apps = [];
    private ?string $defaultApp = null;

    public function addApp(Application $app, bool $isDefault = false): void
    {
        $this->apps[$app->name()] = $app;

        if ($isDefault || $this->defaultApp === null) {
            $this->defaultApp = $app->name();
        }
    }

    public function app(string $name): Application
    {
        if (!isset($this->apps[$name])) {
            throw new \RuntimeException("Unknown app: {$name}");
        }

        return $this->apps[$name];
    }

    public function defaultApp(): Application
    {
        if ($this->defaultApp === null) {
            throw new \RuntimeException('No default app configured.');
        }

        return $this->apps[$this->defaultApp];
    }

    public function apps(): array
    {
        return array_values($this->apps);
    }
}
