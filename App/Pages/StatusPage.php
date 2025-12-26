<?php

namespace App\Pages;

use Framework\Cli\Runtime\PageDefinition;
use Framework\Cli\Ui\Button;
use Framework\Cli\Ui\ButtonRow;
use Framework\Cli\Ui\Element;
use Framework\Cli\Ui\Page;
use Framework\Cli\Ui\Section;
use Framework\Cli\Ui\Table;
use Framework\Cli\Ui\Text;

final class StatusPage extends PageDefinition
{
    public function name(): string
    {
        return 'status';
    }

    public function build(array $args): Element
    {
        $state = $args[0] ?? 'ok';

        return Page::make('Tools Status')
            ->schema([
                Section::make('Health')->schema([
                    Text::make('All systems ready.'),
                    Table::make(['Service', 'State'])->rows([
                        ['CLI', strtoupper($state)],
                        ['PHP', 'READY'],
                    ]),
                    ButtonRow::make([
                        Button::make('Home')->hint('main home')->route('main', 'home'),
                    ]),
                ]),
            ]);
    }
}
