<?php

namespace Framework\Cli\Ui;

final class RenderState
{
    private array $actions = [];

    public function addAction(array $action): void
    {
        $this->actions[] = $action;
    }

    public function actions(): array
    {
        return $this->actions;
    }
}
