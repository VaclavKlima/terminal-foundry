<?php

namespace Framework\Cli\Ui;

final class Input
{
    private string $label;
    private ?string $placeholder;
    private ?string $value;

    private function __construct(string $label, ?string $placeholder, ?string $value)
    {
        $this->label = $label;
        $this->placeholder = $placeholder;
        $this->value = $value;
    }

    public static function make(string $label): self
    {
        return new self($label, null, null);
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

    public function label(): string
    {
        return $this->label;
    }

    public function placeholderText(): ?string
    {
        return $this->placeholder;
    }

    public function valueText(): ?string
    {
        return $this->value;
    }
}
