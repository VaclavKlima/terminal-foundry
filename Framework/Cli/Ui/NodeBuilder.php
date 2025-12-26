<?php

namespace Framework\Cli\Ui;

final class NodeBuilder
{
    public static function nodeFor(Element $element, RenderContext $context): array
    {
        if ($element instanceof NodeElement) {
            return $element->toNode($context);
        }

        return [
            'type' => 'text',
            'text' => $element->render($context),
        ];
    }
}
