<?php

namespace Framework\Cli\Ui;

trait HandlesReactive
{
    private ?\Closure $reactiveHandler = null;

    public function reactive(callable $handler): self
    {
        $this->reactiveHandler = \Closure::fromCallable($handler);
        return $this;
    }

    public function reactiveHandler(): ?\Closure
    {
        return $this->reactiveHandler;
    }
}
