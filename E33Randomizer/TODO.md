# Work to be done
## TODO
* General
  * Finish moving the existing text over to the Resx File.  Need to figure out how to deal with the presets, etc.
  * Finish updating the Items Tab
  * Finish updating the Location Tab
  * Finish updating the Skill Tab
  * Finish updating the Misc Tab
  * Add help tooltips throughout
  * Finish incorporating the background assets
  * Create better cross-platform build (to fetch the both the linux and windows `retoc` and `uesave`)
  * Investigate switching to `AvaloniaDictionary<T,TV>` over the custom `ObeservableCollectionWithChildListener<T>` 
  * Continue to iterate on theme designs
  * Figure out why `FrequencyAdjustmentsCategoryComboBox` doesn't show the error icon even when it's selected item has an error.
* Individual Containers Window
  * All the things
* Custom Placements Window
  * Design something for the "oops all" section which presently doesn't exist.
  * Try to fix the issue with Select Category getting unselected when the replacements are changed. (I know why it's happening, but not sure how to fix it yet) 

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
