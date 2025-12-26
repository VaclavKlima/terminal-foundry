<?php

namespace Framework\Cli\Ui;

final class ButtonRow implements Element, NodeElement
{
    private array $buttons;

    public function __construct(array $buttons)
    {
        $this->buttons = $buttons;
    }

    public static function make(array $buttons): self
    {
        return new self($buttons);
    }

    public function render(RenderContext $context): string
    {
        $palette = $context->palette();
        $primary = $palette['primary'] ?? '';
        $muted = $palette['muted'] ?? '';
        $reset = $palette['reset'] ?? '';

        $parts = [];
        foreach ($this->buttons as $button) {
            if (is_string($button)) {
                $button = Button::make($button);
            }

            if (!$button instanceof Button) {
                continue;
            }

            $color = $button->isActive() ? $primary : $muted;
            $label = $button->label();
            $hint = $button->hintText();
            $actionArgs = $button->actionArgs();
            if ($actionArgs !== null) {
                $context->addAction([
                    'label' => $label,
                    'args' => $actionArgs,
                ]);
            }
            $text = '[' . $label . ']';
            if ($hint !== null && $hint !== '') {
                $text .= ' ' . $hint;
            }
            $parts[] = $color . $text . $reset;
        }

        return implode('  ', $parts);
    }

    public function toNode(RenderContext $context): array
    {
        $buttons = [];
        foreach ($this->buttons as $button) {
            if (is_string($button)) {
                $button = Button::make($button);
            }

            if (!$button instanceof Button) {
                continue;
            }

            $buttons[] = [
                'label' => $button->label(),
                'hint' => $button->hintText(),
                'args' => $button->actionArgs(),
            ];
        }

        return [
            'type' => 'buttonRow',
            'buttons' => $buttons,
        ];
    }
}
