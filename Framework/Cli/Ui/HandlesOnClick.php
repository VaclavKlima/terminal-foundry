<?php

namespace Framework\Cli\Ui;

trait HandlesOnClick
{
    private ?\Closure $actionHandler = null;

    public function onClick(callable $handler): self
    {
        $this->actionHandler = \Closure::fromCallable($handler);
        $this->clearActionArgs();
        return $this;
    }

    public function actionHandler(): ?\Closure
    {
        return $this->actionHandler;
    }

    abstract protected function clearActionArgs(): void;
}
