<?php

namespace Framework\Cli\Ui;

trait HandlesReactive
{
    private ?\Closure $reactiveHandler = null;
    private bool $reactiveEnabled = false;

    public function reactive(callable $handler = null): self
    {
        $this->reactiveEnabled = true;
        $this->reactiveHandler = $handler !== null ? \Closure::fromCallable($handler) : null;
        return $this;
    }

    public function reactiveHandler(): ?\Closure
    {
        return $this->reactiveHandler;
    }

    public function reactiveEnabled(): bool
    {
        return $this->reactiveEnabled;
    }
}
