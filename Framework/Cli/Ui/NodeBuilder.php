<?php

namespace Framework\Cli\Ui;

final class NodeBuilder
{
    public static function nodeFor(Element $element, RenderContext $context): array
    {
        if ($element instanceof NodeElement) {
            $node = $element->toNode($context);
            if (!isset($node['id'])) {
                $type = isset($node['type']) ? (string) $node['type'] : 'node';
                $node['id'] = $context->nextNodeId($type);
            }
            return $node;
        }

        return [
            'type' => 'text',
            'text' => $element->render($context),
            'id' => $context->nextNodeId('text'),
        ];
    }
}
