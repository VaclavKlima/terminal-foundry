<?php

namespace Framework\Cli\Ui;

final class InputRow implements Element
{
    private array $inputs;

    public function __construct(array $inputs)
    {
        $this->inputs = $inputs;
    }

    public function render(RenderContext $context): string
    {
        $palette = $context->palette();
        $muted = $palette['muted'] ?? '';
        $reset = $palette['reset'] ?? '';

        $lines = [];
        foreach ($this->inputs as $input) {
            if (is_string($input)) {
                $input = Input::make($input);
            }

            if (!$input instanceof Input) {
                continue;
            }

            $value = $input->valueText();
            if ($value === null || $value === '') {
                $value = $input->placeholderText() ?? '';
                $value = $value !== '' ? $muted . $value . $reset : '';
            }

            $lines[] = sprintf('%s: %s', $input->label(), $value);
        }

        return implode(PHP_EOL, $lines);
    }
}
