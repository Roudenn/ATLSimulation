## General stuff

ui-options-title = Настройки
ui-options-tab-graphics = Графика
ui-options-tab-controls = Управление
ui-options-tab-server = Сервер

ui-options-apply = Сохранить и применить
ui-options-reset-all = Очистить изменённые
ui-options-default = Очистить всё по умолчанию

ui-options-value-percent = { TOSTRING($value, "P0") }
## Graphics menu

ui-options-display-label = Окно
ui-options-quality-label = Качество
ui-options-misc-label = Другое
ui-options-interface-label = Интерфейс


ui-options-vsync = VSync
ui-options-fullscreen = Fullscreen
ui-options-lighting-label = Lighting Quality:
ui-options-lighting-very-low = Very Low
ui-options-lighting-low = Low
ui-options-lighting-medium = Medium
ui-options-lighting-high = High
ui-options-scale-label = UI Scale:
ui-options-scale-auto = Automatic ({ TOSTRING($scale, "P0") })
ui-options-scale-75 = 75%
ui-options-scale-100 = 100%
ui-options-scale-125 = 125%
ui-options-scale-150 = 150%
ui-options-scale-175 = 175%
ui-options-scale-200 = 200%
ui-options-vp-stretch = Stretch viewport to fit game window
ui-options-vp-scale = Fixed viewport scale:
ui-options-vp-scale-value = x{ $scale }
ui-options-vp-integer-scaling = Prefer integer scaling (might cause black bars/clipping)
ui-options-vp-integer-scaling-tooltip = If this option is enabled, the viewport will be scaled using an integer value
                                        at specific resolutions. While this results in crisp textures, it also often
                                        means that black bars appear at the top/bottom of the screen or that part
                                        of the viewport is not visible.
ui-options-filter-label = Scaling filter:
ui-options-filter-nearest = Nearest (no smoothing)
ui-options-filter-bilinear = Bilinear (smoothed)
ui-options-vp-vertical-fit = Vertical viewport fitting
ui-options-vp-vertical-fit-tooltip = When enabled, the main viewport will ignore the horizontal axis entirely when
                                     fitting to your screen. If your screen is smaller than the viewport, then this
                                     will cause the viewport to be cut off on the horizontal axis.
ui-options-vp-low-res = Low-resolution viewport
ui-options-parallax-low-quality = Low-quality Parallax (background)
ui-options-fps-counter = Show FPS counter
ui-options-vp-width = Viewport width:

## Controls menu

ui-options-binds-reset-all = Очистить ВСЕ привязки
ui-options-binds-explanation = ЛКМ — изменить привязку, ПКМ — убрать привязку
ui-options-unbound = Пусто
ui-options-bind-reset = Очистить
ui-options-key-prompt = Нажмите клавишу...

ui-options-header-movement = Движение
ui-options-header-camera = Камера
ui-options-header-interaction-basic = Базовое взаимодействие
ui-options-header-interaction-adv = Продвинутое взаимодействие
ui-options-header-ui = Пользовательский интерфейс
ui-options-header-misc = Дополнительное
ui-options-header-map-editor = Редактор карт
ui-options-header-dev = Разработка
ui-options-header-general = Общее
ui-options-header-text-cursor = Курсор текста
ui-options-header-text-cursor-select = Выбор текста
ui-options-header-text-edit = Редактирование текста
ui-options-header-text-other = Другой ввод текста

ui-options-hotkey-keymap = Использовать раскладку US QWERTY
ui-options-hotkey-toggle-walk = Прееключение ходьбы

ui-options-function-move-up = Двигаться вверх
ui-options-function-move-left = Двигаться влево
ui-options-function-move-down = Двигаться вниз
ui-options-function-move-right = Двигаться вправо
ui-options-function-walk = Ходить

ui-options-function-camera-rotate-left = Повернуть влево
ui-options-function-camera-rotate-right = Повернуть вправо
ui-options-function-camera-reset = Очистить поворот
ui-options-function-zoom-in = Приблизить
ui-options-function-zoom-out = Отдалить
ui-options-function-reset-zoom = Очистить приближение

ui-options-function-examine-entity = Осмотреть

ui-options-function-rotate-object-clockwise = Повернуть по часовой
ui-options-function-rotate-object-counterclockwise = Повернуть против часовой
ui-options-function-flip-object = Перевернуть

ui-options-function-open-entity-spawn-window = Open entity spawn menu
ui-options-function-open-tile-spawn-window = Open tile spawn menu
ui-options-function-window-close-all = Close all windows
ui-options-function-window-close-recent = Close recent window
ui-options-function-show-escape-menu = Toggle game menu
ui-options-function-escape-context = Close recent window or toggle game menu

ui-options-function-take-screenshot = Сделать снимок экрана
ui-options-function-take-screenshot-no-ui = Сделать снимок экрана (без интерфейса)
ui-options-function-toggle-fullscreen = Переключить полноэкранный режим

ui-options-function-editor-place-object = Поставить объект
ui-options-function-editor-cancel-place = Отменить установку
ui-options-function-editor-grid-place = Поставить на сетке
ui-options-function-editor-line-place = Поставить в линию
ui-options-function-editor-rotate-object = Повернуть
ui-options-function-editor-flip-object = Перевернуть
ui-options-function-editor-copy-object = Копировать

ui-options-function-show-debug-console = Открыть консоль
ui-options-function-show-debug-monitors = Показать статистику откладки
ui-options-function-inspect-entity = Осмотреть переменные сущности
ui-options-function-inspect-entity-tooltip = Open a ViewVariables window for the entity your mouse is currently hovering over.
ui-options-function-inspect-server-component = Осмотреть компонент Сервера
ui-options-function-inspect-server-component-tooltip = Open a ViewVariables window with the server component set by the "quickinspect" command for the entity your mouse is currently hovering over.
ui-options-function-inspect-client-component = Осмотреть компонент Клиента
ui-options-function-inspect-client-component-tooltip = Open a ViewVariables window with the client component set by the "quickinspect" command for the entity your mouse is currently hovering over.
ui-options-function-hide-ui = Спрятать интерфейс
ui-options-function-inspect-subgrid-element = Осмотреть клетку

ui-options-function-text-cursor-left = Move cursor left
ui-options-function-text-cursor-right = Move cursor right
ui-options-function-text-cursor-up = Move cursor up
ui-options-function-text-cursor-down = Move cursor down
ui-options-function-text-cursor-word-left = Move cursor left by word
ui-options-function-text-cursor-word-right = Move cursor right by word
ui-options-function-text-cursor-begin = Move cursor to beginning
ui-options-function-text-cursor-end = Move cursor to end
ui-options-function-text-cursor-select = Select text
ui-options-function-text-cursor-select-left = Expand selection left
ui-options-function-text-cursor-select-right = Expand selection right
ui-options-function-text-cursor-select-up = Expand selection up
ui-options-function-text-cursor-select-down = Expand selection down
ui-options-function-text-cursor-select-word-left = Expand selection left by word
ui-options-function-text-cursor-select-word-right = Expand selection right by word
ui-options-function-text-cursor-select-begin = Expand selection to beginning
ui-options-function-text-cursor-select-end = Expand selection to end
ui-options-function-text-backspace = Backspace
ui-options-function-text-delete = Delete
ui-options-function-text-word-backspace = Backspace word
ui-options-function-text-word-delete = Delete word
ui-options-function-text-newline = Newline
ui-options-function-text-submit = Submit
ui-options-function-multiline-text-submit = Submit multiline
ui-options-function-text-select-all = Select all
ui-options-function-text-copy = Copy
ui-options-function-text-cut = Cut
ui-options-function-text-paste = Paste
ui-options-function-text-history-prev = Previous from history
ui-options-function-text-history-next = Next from history
ui-options-function-text-release-focus = Release focus
ui-options-function-text-scroll-to-bottom = Scroll to bottom
ui-options-function-text-tab-complete = Tab completion
ui-options-function-text-complete-next = Complete next
ui-options-function-text-complete-prev = Complete previous
