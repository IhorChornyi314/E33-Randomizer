local function loadMapping()
    local mapping = {}

    local st = StaticFindObject("/Game/YourMod/ST_LocationMapping.ST_LocationMapping")
    if not st or not st:IsValid() then
        print("[LocationRandomizer] ERROR: Could not find ST_LocationMapping\n")
        return mapping
    end

    local keys = st:GetKeys()
    for i = 1, #keys do
        local key = keys[i]:ToString()
        local value = st:GetEntry(key):ToString()
        mapping[key] = value
    end

    return mapping
end

return loadMapping()