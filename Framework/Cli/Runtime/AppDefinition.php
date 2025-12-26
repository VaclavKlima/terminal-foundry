<?php

namespace Framework\Cli\Runtime;

abstract class AppDefinition
{
    abstract public function name(): string;

    public function uiOptions(): array
    {
        return [];
    }

    abstract protected function registerPages(Application $app): void;

    final public function build(): Application
    {
        $app = new Application($this->name(), $this->uiOptions());
        $this->registerPages($app);

        return $app;
    }
}
