# Work to be done
## TODO
* General
  * Create better cross-platform build (to fetch the both the linux and windows `retoc` and `uesave`)
  * Investigate switching to `AvaloniaDictionary<T,TV>` over the custom `ObeservableCollectionWithChildListener<T>` 
  * Continue to iterate on theme designs
* Look into creating better design time data sets that don't require the full controller initializations to function.  (would be much faster)
  * I think a quick little console app that would initialize the full data set then generating a set of classes that had all of it hard coded would work pretty well.  It'd only need to be run when you wanted to regenerate that data if changes were made.
  * Applies to individual Containers, custom placements, and the main window.
* Individual Containers Window
    * Update the Resources.resx entries for the different object types
    * Maybe a little tweaking in the styles, not settled on the button layout at the bottom yet.
* Custom Placements Window
  * Update the Resources.resx entries for the different object types 
  * Design something for the "oops all" section which presently doesn't exist.
  * Try to fix the issue with Select Category getting unselected when the replacements are changed. (I know why it's happening, but not sure how to fix it yet.  It's due to the use of tuples, there is a comment around that logic that explains more.)
  * Figure out why `FrequencyAdjustmentsCategoryComboBox` doesn't show the error icon even when it's selected item has an error.
* Consider a Help Page baked right into it? or just link to the help docs on the mod site?...not sure.

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
    * Backgrounds for windows to give them some contrast and differentiation.
    * Custom Scrollbars
  * Add help tooltips throughout
  * Convert to Cross-Platform builds
  * Convert from Newtonsoft.Json to System.Text.Json with source generators
  * Remove / Fix most non-AOT compatible code (sadly not sure that we can fully get to AOT yet for the time being due to the UAssetsAPI)
  * Fixed pathing issues between Windows and Linux
  * Redesigned the "main" screen's layout with the new core theme/design principals
  * Finish updating the Items Tab
  * Finish updating the Location Tab
  * Finish updating the Skill Tab
  * Finish updating the Misc Tab
  * Create themed Message Dialog box that can be used for both errors and confirmations.
  * Moved everything other than data driven values like presets/names of things into Localizable Resource file.
    * Including warnings and error messages
* Custom Placements
  * Completely redesigned UX
    * Now uses tabs and makes the sub sections much less compressed so it's easier to see what is configured.
    * Adds searching to several boxes to make managing the very large list easier.
    * Makes interactions behave consistently (specifically making the Excluded/NotRandomized boxes behave like other list boxes)
  * Redesigned the binding/viewmodel setup to make it require less steps for synchronization
    * Including less manual control construction in the code behind
* Individual Containers Window
  * Full UX refresh.  Removed the "Add" button, it's now just an event that auto happens when selecting the entity from the list
  * Fixed all the bindings after switching to Avalonia.  This window had much less indirection and complex bindings that needed to be resolved, so alot less changed there.
  * Added the help tooltips where they made sense
