# Work to be done
## TODO
* General
  * Possibly setup the ability for using resx files for strings so that an option to internationalize is there in the future.
  * Finish updating the Items Tab
  * Finish updating the Location Tab
  * Finish updating the Skill Tab
  * Finish updating the Misc Tab
  * Add help tooltips throughout
  * See about extracting the menu backgrounds from the game and incorporating them.
  * Create better cross-platform build (to fetch the both the linux and windows `retoc` and `uesave`)
  * Investigate switching to `AvaloniaDictionary<T,TV>` over the custom `ObeservableCollectionWithChildListener<T>` 
  * Continue to iterate on theme designs
* Individual Containers Window
  * All the things
* Custom Placements Window
  * Finish implementing the tooltips throughout
  * Finish Frequency Adjustments cleanup.
  * Continue to tweak the layout/etc (I'm still not completely happy with everything yet.) Including the JSON flyout colors/theming.
  * Fix the weird thing when a new row is added to the Custom Placement section and it squishes the row.
  * Re-skin the "delete/remove" button
  * Re-skin the "add Placement row" button
  * Design something for the "oops all" section which presently doesn't exist.

## Done
* General
  * Convert to Avalonia 
  * Build core theme/styles
    * Fonts
    * Colors
    * Layout
    * Buttons
    * Windows
    * Popups / Tooltips
  * Convert to Cross-Platform builds
  * Convert from Newtonsoft.Json to System.Text.Json with source generators
  * Remove / Fix most non-AOT compatible code (sadly not sure that we can fully get to AOT yet for the time being due to the UAssetsAPI)
  * Fixed pathing issues between Windows and Linux
  * Redesigned the "main" screen's layout with the new core theme/design principals
* Custom Placements
  * Completely redesigned UX
    * Now uses tabs and makes the sub sections much less compressed so it's easier to see what is configured.
    * Adds searching to several boxes to make managing the very large list easier.
    * Makes interactions behave consistently (specifically making the Excluded/NotRandomized boxes behave like other list boxes)
  * Redesigned the binding/viewmodel setup to make it require less steps for synchronization
    * Including less manual control construction in the code behind
