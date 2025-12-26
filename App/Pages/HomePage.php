<?php

namespace App\Pages;

use Framework\Cli\Runtime\PageDefinition;
use Framework\Cli\Ui\Button;
use Framework\Cli\Ui\Card;
use Framework\Cli\Ui\Element;
use Framework\Cli\Ui\Page;
use Framework\Cli\Ui\Section;
use Framework\Cli\Ui\Table;
use Framework\Cli\Ui\Text;
use Framework\Cli\Ui\TextInput;

final class HomePage extends PageDefinition
{
    public function name(): string
    {
        return 'home';
    }

    public function build(array $args): Element
    {
        $argsText = $args === [] ? 'No args provided.' : implode(' ', $args);

        return Page::make('Main App')
            ->schema([
                Section::make('Welcome')->schema([
                    Text::make('Console UI booted. Use "main about" or "tools status".'),
                    Card::make('Next')->schema([
                        Text::make('Jump to other pages using the commands below.'),
                        Button::make('Tools')->hint('tools status')->route('tools', 'status'),
                    ]),
                    Table::make(['Item', 'Value'])->rows([
                        ['Args', $argsText],
                    ]),
                    Button::make('About')->hint('main about')->route('main', 'about'),
                    Button::make('Tools')->hint('tools status')->route('tools', 'status'),
                ]),
                Section::make('Inputs')->schema([
                    TextInput::make('name')
                        ->label('Name')
                        ->required()
                        ->helperText('Internal name for administration purposes only')
                        ->columnSpan(2),
                    TextInput::make('mode')->label('Mode')->placeholder('debug'),
                ]),
            ]);
    }
}
