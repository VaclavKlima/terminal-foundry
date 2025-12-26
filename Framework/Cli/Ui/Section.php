<?php

namespace Framework\Cli\Ui;

final class Section implements Element, NodeElement
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

    public function card(string $title, callable $builder): self
    {
        $card = new Card($title);
        $builder($card);
        $this->blocks[] = $card;
        return $this;
    }

    public function table(array $headers, array $rows = []): self
    {
        $table = Table::make($headers, $rows);
        $this->blocks[] = $table;
        return $this;
    }

    public function buttons(array $buttons): self
    {
        $this->blocks[] = new ButtonRow($buttons);
        return $this;
    }

    public function inputs(array $inputs): self
    {
        $this->blocks[] = new InputRow($inputs);
        return $this;
    }

    public function add(Element $element): self
    {
        $this->blocks[] = $element;
        return $this;
    }

    public function render(RenderContext $context): string
    {
        $palette = $context->palette();
        $muted = $palette['muted'] ?? '';
        $reset = $palette['reset'] ?? '';

        $lines = [];
        if ($this->title !== '') {
            $lines[] = $muted . strtoupper($this->title) . $reset;
        }

        foreach ($this->blocks as $block) {
            if ($block instanceof Button) {
                $block = new ButtonRow([$block]);
            }
            $output = $block->render($context);
            if ($output !== '') {
                $lines[] = $this->title !== '' ? self::indent($output, '  ') : $output;
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
            'type' => 'section',
            'title' => $this->title,
            'children' => $children,
        ];
    }

    public static function indent(string $text, string $prefix): string
    {
        $lines = preg_split('/\r\n|\r|\n/', $text);
        foreach ($lines as $index => $line) {
            $lines[$index] = $prefix . $line;
        }

        return implode(PHP_EOL, $lines);
    }
}
