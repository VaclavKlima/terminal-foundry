<?php

namespace Framework\Cli\Ui;

final class Card implements Element, NodeElement
{
    private string $title;
    private array $blocks = [];

    public function __construct(string $title)
    {
        $this->title = $title;
    }

    public static function make(string $title): self
    {
        return new self($title);
    }

    public function schema(array $blocks): self
    {
        $this->blocks = $blocks;
        return $this;
    }

    public function text(string $text): self
    {
        $this->blocks[] = new Text($text);
        return $this;
    }

    public function add(Element $element): self
    {
        $this->blocks[] = $element;
        return $this;
    }

    public function render(RenderContext $context): string
    {
        $lines = [];
        $lines[] = '[' . $this->title . ']';

        foreach ($this->blocks as $block) {
            if ($block instanceof Button) {
                $block = new ButtonRow([$block]);
            }
            $output = $block->render($context);
            if ($output !== '') {
                $lines[] = Section::indent($output, '  ');
            }
        }

        return implode(PHP_EOL, $lines);
    }

    public function toNode(RenderContext $context): array
    {
        $children = [];
        foreach ($this->blocks as $block) {
            if ($block instanceof Button) {
                $block = new ButtonRow([$block]);
            }
            $children[] = NodeBuilder::nodeFor($block, $context);
        }

        return [
            'type' => 'card',
            'title' => $this->title,
            'children' => $children,
        ];
    }
}
