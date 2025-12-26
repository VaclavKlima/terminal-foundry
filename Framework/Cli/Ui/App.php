<?php

namespace Framework\Cli\Ui;

final class App implements Element, NodeElement
{
    private ?int $columns;
    private ?string $colorScheme;
    private ?array $palette;
    private array $children = [];

    public function __construct(array $options = [])
    {
        $this->columns = isset($options['columns']) ? (int) $options['columns'] : null;
        $this->colorScheme = $options['colorScheme'] ?? null;
        $this->palette = $options['palette'] ?? null;
    }

    public function add(Element $child): void
    {
        $this->children[] = $child;
    }

    public function render(RenderContext $context): string
    {
        $context = $context->withOverrides($this->columns, $this->colorScheme, $this->palette);

        $chunks = [];
        foreach ($this->children as $child) {
            $output = $child->render($context);
            if ($output !== '') {
                $chunks[] = $output;
            }
        }

        return implode(PHP_EOL, $chunks);
    }

    public function toNode(RenderContext $context): array
    {
        $context = $context->withOverrides($this->columns, $this->colorScheme, $this->palette);
        $children = [];
        foreach ($this->children as $child) {
            $children[] = NodeBuilder::nodeFor($child, $context);
        }

        return [
            'type' => 'app',
            'children' => $children,
        ];
    }
}
