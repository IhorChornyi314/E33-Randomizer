local function loadMapping()
    local mapping = {}

    local stPath = "/Game/StringTables/UI/ST_UI_ModalPopup.ST_UI_ModalPopup"
    local st = StaticFindObject(stPath)

    if not st or not st:IsValid() then
        return nil 
    end

    local KST_Lib = StaticFindObject("/Script/Engine.Default__KismetStringTableLibrary")
    if not KST_Lib or not KST_Lib:IsValid() then
        print("[LocationRandomizer] ERROR: Could not find Kismet String Table Library!\n")
        return nil
    end

    local tableId = FName(stPath)

    local keysArray = KST_Lib:GetKeysFromStringTable(tableId)
    
    if not keysArray then
        print("[LocationRandomizer] ERROR: Could not retrieve keys from Kismet Library.\n")
        return nil
    end

    for i, keyParam in ipairs(keysArray) do
        local keyFString = keyParam:get()
        local keyStr = keyFString:ToString()

        if keyStr and keyStr:find(":") then 
            local valFString = KST_Lib:GetTableEntrySourceString(tableId, keyStr)
            
            local valStr = valFString:ToString()
            
            mapping[keyStr] = valStr
        end
    end

    print("[LocationRandomizer] Successfully loaded Mapping Data!\n")
    return mapping
end

return loadMapping