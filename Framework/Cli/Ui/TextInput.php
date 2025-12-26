<?php

namespace Framework\Cli\Ui;

final class TextInput implements Element, NodeElement
{
    private string $name;
    private ?string $label = null;
    private ?string $helperText = null;
    private ?string $placeholder = null;
    private ?string $value = null;
    private bool $required = false;
    private ?int $columnSpan = null;

    private function __construct(string $name)
    {
        $this->name = $name;
    }

    public static function make(string $name): self
    {
        return new self($name);
    }

    public function label(string $label): self
    {
        $this->label = $label;
        return $this;
    }

    public function helperText(string $text): self
    {
        $this->helperText = $text;
        return $this;
    }

    public function placeholder(string $placeholder): self
    {
        $this->placeholder = $placeholder;
        return $this;
    }

    public function value(string $value): self
    {
        $this->value = $value;
        return $this;
    }

    public function required(bool $required = true): self
    {
        $this->required = $required;
        return $this;
    }

    public function columnSpan(int $span): self
    {
        $this->columnSpan = $span;
        return $this;
    }

    public function render(RenderContext $context): string
    {
        $palette = $context->palette();
        $muted = $palette['muted'] ?? '';
        $reset = $palette['reset'] ?? '';

        $label = $this->label ?? $this->name;
        $suffix = $this->required ? ' *' : '';
        $value = $this->value;
        $placeholder = $this->placeholder;

        if ($value === null || $value === '') {
            $value = $placeholder !== null ? $muted . $placeholder . $reset : '';
        }

        $lines = [];
        $lines[] = $label . $suffix . ': ' . $value;
        if ($this->helperText !== null && $this->helperText !== '') {
            $lines[] = $muted . $this->helperText . $reset;
        }

        return implode(PHP_EOL, $lines);
    }

    public function toNode(RenderContext $context): array
    {
        return [
            'type' => 'textInput',
            'name' => $this->name,
            'label' => $this->label,
            'helperText' => $this->helperText,
            'placeholder' => $this->placeholder,
            'value' => $this->value,
            'required' => $this->required,
            'columnSpan' => $this->columnSpan,
        ];
    }
}
