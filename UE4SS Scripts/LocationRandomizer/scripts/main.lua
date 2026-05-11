local UEHelpers = require("UEHelpers")
local loadMappingData = require("mapping")
local mappingData = nil

local targetFunction = "/Game/jRPGTemplate/Blueprints/Basics/FL_jRPG_CustomFunctionLibrary.FL_jRPG_CustomFunctionLibrary_C:Change Map Internal"

local function hookCallback(Context, LevelDestination, SpawnPointTag)
    if not mappingData then
        mappingData = loadMappingData()
        if not mappingData then
            print("[LocationRandomizer] ERROR: Mapping data not loaded yet. Is the String Table referenced in memory?\n")
            return
        end
    end

    local levelDest = LevelDestination:get().LevelAssetName_85_BF09694C41CC0444295731A40341A5F9:ToString()
    local currentTag = SpawnPointTag:get().TagName:ToString()
    local lookupKey = levelDest .. ":" .. currentTag

    print("[LocationRandomizer] Loading Map: " .. lookupKey .. "\n")

    local newDest = mappingData[lookupKey]
    if not newDest then return end

    print("[LocationRandomizer] Found override: " .. newDest .. "\n")

    local newMap, newTagStr = newDest:match("([^:]+):(.+)")
    if not newMap or not newTagStr then return end

    local CustomGI = FindFirstOf("BP_jRPG_GI_Custom_C")
    if not CustomGI or not CustomGI:IsValid() then
        print("[LocationRandomizer] ERROR: Could not find GI\n")
        return
    end

    CustomGI.LevelDestinationAssetName = FName(newMap)
    CustomGI.SpawnPointTag.TagName = FName(newTagStr)

    print("[LocationRandomizer] Overrode Map: " .. newMap .. ":" .. newTagStr .. "\n")
end

local function tryRegisterHook()
    local success, err = pcall(function()
        RegisterHook(targetFunction, hookCallback)
    end)

    if success then
        print("[LocationRandomizer] Hook registered successfully.\n")
    else
        print("[LocationRandomizer] Hook registration failed, retrying...\n")
        ExecuteWithDelay(1000, tryRegisterHook)
    end
end

tryRegisterHook()