<?php

namespace Framework\Cli\Ui;

final class Select implements Element, NodeElement
{
    use HandlesReactive;

    private string $name;
    private ?string $label = null;
    private ?string $helperText = null;
    private array $options = [];
    private ?string $value = null;
    private bool $required = false;

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

    public function options(array $options): self
    {
        $this->options = $options;
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

    public function render(RenderContext $context): string
    {
        $palette = $context->palette();
        $muted = $palette['muted'] ?? '';
        $reset = $palette['reset'] ?? '';

        $label = $this->label ?? $this->name;
        $suffix = $this->required ? ' *' : '';
        $value = $this->value;

        if ($value === null || $value === '') {
            $value = $muted . 'select' . $reset;
        } elseif (isset($this->options[$value])) {
            $value = (string) $this->options[$value];
        }

        $lines = [];
        $lines[] = $label . $suffix . ': ' . $value;

        if ($this->helperText !== null && $this->helperText !== '') {
            $lines[] = $muted . $this->helperText . $reset;
        }

        if ($this->options !== []) {
            $labels = [];
            foreach ($this->options as $key => $optionLabel) {
                $labels[] = $key . '=' . $optionLabel;
            }
            $lines[] = $muted . 'Options: ' . implode(', ', $labels) . $reset;
        }

        return implode(PHP_EOL, $lines);
    }

    public function toNode(RenderContext $context): array
    {
        $onChange = null;
        $handler = $this->reactiveHandler();
        if ($handler !== null && getenv('APP_WORKER') === '1') {
            $onChange = \Framework\Cli\Runtime\ActionRegistry::register($handler);
        }

        return [
            'type' => 'select',
            'name' => $this->name,
            'label' => $this->label,
            'helperText' => $this->helperText,
            'options' => $this->options,
            'value' => $this->value,
            'required' => $this->required,
            'onChangeAction' => $onChange,
        ];
    }
}
