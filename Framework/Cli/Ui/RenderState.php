<?php

namespace Framework\Cli\Ui;

final class RenderState
{
    private array $actions = [];
    private int $nodeCounter = 0;

    public function addAction(array $action): void
    {
        $this->actions[] = $action;
    }

    public function actions(): array
    {
        return $this->actions;
    }

    public function nextNodeId(string $prefix): string
    {
        $this->nodeCounter++;
        return $prefix . '-' . $this->nodeCounter;
    }
}
