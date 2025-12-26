<?php

namespace Framework\Cli\Ui;

final class Renderer
{
    public function render(Element $root): string
    {
        $context = RenderContext::fromEnvironment();
        return $root->render($context);
    }

    public function renderWithState(Element $root): array
    {
        $state = new RenderState();
        $context = RenderContext::fromEnvironment($state);
        $text = $root->render($context);

        return [
            'text' => $text,
            'actions' => $state->actions(),
        ];
    }

    public function renderNodes(Element $root): array
    {
        $state = new RenderState();
        $context = RenderContext::fromEnvironment($state);
        return NodeBuilder::nodeFor($root, $context);
    }
}
