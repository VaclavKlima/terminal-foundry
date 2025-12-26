<?php

namespace Framework\Cli\Ui;

final class Text implements Element, NodeElement
{
    private string $text;

    public function __construct(string $text)
    {
        $this->text = $text;
    }

    public static function make(string $text): self
    {
        return new self($text);
    }

    public function render(RenderContext $context): string
    {
        $width = $context->columns();
        if ($width <= 1) {
            return $this->text;
        }

        return wordwrap($this->text, $width, PHP_EOL, true);
    }

    public function toNode(RenderContext $context): array
    {
        return [
            'type' => 'text',
            'text' => $this->text,
        ];
    }
}
