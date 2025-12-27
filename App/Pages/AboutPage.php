<?php

namespace App\Pages;

use Framework\Cli\Runtime\PageDefinition;
use Framework\Cli\Ui\Button;
use Framework\Cli\Ui\ButtonRow;
use Framework\Cli\Ui\Element;
use Framework\Cli\Ui\Page;
use Framework\Cli\Ui\Section;
use Framework\Cli\Ui\Select;
use Framework\Cli\Ui\Table;
use Framework\Cli\Ui\Text;

final class AboutPage extends PageDefinition
{
    public function name(): string
    {
        return 'about';
    }

    public function build(array $args): Element
    {
        $version = $args[0] ?? 'dev';

        return Page::make('About')
            ->schema([
                Section::make('Runtime')->schema([
                    Text::make('This is a multi-app CLI runtime demo.'),
                    Table::make(['Key', 'Value'])->rows([
                        ['Version', $version],
                        ['Framework', 'Cli'],
                    ]),
                    Select::make('theme')
                        ->label('Theme')
                        ->options([
                            'dark' => 'Dark',
                            'light' => 'Light',
                        ])
                        ->helperText('UI theme preference'),
                    ButtonRow::make([
                        Button::make('Home')->hint('main home')
                            ->onClick(fn () => \Framework\Cli\Runtime\Router::to('main', 'home')),
                    ]),
                ]),
            ]);
    }
}
