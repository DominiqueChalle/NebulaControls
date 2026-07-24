# NebulaControls Roadmap

NebulaControls is an actively developed WPF control library.

The current public package is usable as a beta, but the project is still evolving through visual review, keyboard and mouse behavior testing, and real usage examples.

## Current Focus

The main focus is the new documentation-first demo.

This demo is being built progressively so each control page shows:

- a real interactive preview
- the XAML used to create the visible controls
- the C# used to read values, validate input, or react to user actions

The older demo remains available as a validation lab while the new demo/docs become the main public showcase.

## Recently Validated Areas

- Core themes: `NebulaDarkPurple`, `NebulaDarkBlue`, `NebulaLightPurple`
- Runtime theme switching through `NebulaThemeManager`
- `NebulaWindow` shell behavior
- Text inputs, password inputs, email input, search input
- Buttons and choice inputs
- Pickers: custom date and time pickers
- DataGrid editing, add-row behavior, required cells, and SQLite demo scenario
- Toast host behavior
- Keyboard navigation improvements across several controls

## Documentation Demo Integration Order

The current planned order for the new demo/docs is:

1. `NebulaSlider`
2. `NebulaProgressBar`
3. `NebulaListBox`
4. `NebulaTreeView`
5. `NebulaTabControl`
6. `NebulaDataGrid`
7. `NebulaToast` and `NebulaToastHost`
8. `NebulaDialog`
9. `NebulaMessageBox`
10. `NebulaAlert`
11. `NebulaBadge`
12. `NebulaChip`
13. `NebulaRating`
14. `NebulaAvatar`
15. `NebulaSideBar`
16. `NebulaWindow`

## Planned Improvements

- Continue moving reusable behavior from the demo into the controls themselves
- Improve the documentation preview until it can become the main public demo
- Add more real-world examples for common WPF application scenarios
- Add sorting, filtering, and row deletion improvements to `NebulaDataGrid`
- Revisit `NebulaTabControl` in a focused phase if old-school overlapping tabs need deeper refinement
- Revisit picker visuals for a future V3 polish pass
- Keep future controls selective and practical instead of adding a large number of unfinished controls

## Future Control Candidates

Potential future controls include:

- `NebulaFilePicker`
- `NebulaFolderPicker`
- `NebulaPagination`
- `NebulaBreadcrumb`
- `NebulaStatusBar`
- `NebulaToolbar`
- `NebulaCommandBar`
- `NebulaInfoBar`
- `NebulaLoadingOverlay`
- `NebulaEmptyState`
- `NebulaStepper`
- `NebulaWizard`
- `NebulaColorPicker`
- `NebulaMaskedTextBox`
- `NebulaTagBox`

These are candidates, not promises. Controls are added only when they fit the library direction and can be validated properly.

## Release Strategy

Beta packages are not published after every single change.

A new beta package is planned when the library and the documentation demo contain a coherent set of useful, tested controls. The next beta should make it easier for external developers to understand how to use NebulaControls without reading the source code first.

The stable `1.0.0` release will be prepared after enough beta feedback and after the main control surface feels consistent.
