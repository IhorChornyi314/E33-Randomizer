local M = {}

local POLL_MS = 250
local SLOT_NAME = "CharacterHardcodedName_2_2A63D4C5433428900D69748563F50580"
local started = false
local lastReported = nil

local function unwrap(v)
    if type(v) == "userdata" and v.get then
        local ok, r = pcall(function() return v:get() end)
        if ok then return r end
    end
    return v
end

local function str(v)
    v = unwrap(v)
    if type(v) == "userdata" and v.ToString then
        local ok, r = pcall(function() return v:ToString() end)
        if ok then return r end
    end
    return tostring(v)
end

local function get_live_gi()
    local all = FindAllOf("BP_jRPG_GI_Custom_C")
    if not all then return nil end
    for _, g in ipairs(all) do
        if g:IsValid() and not g:GetFName():ToString():find("^Default__") then
            return g
        end
    end
    return nil
end

local function check_world_character()
    local gi = get_live_gi()
    if not gi then return end

    local current = str(gi.CurrentCharacterWorld)
    if current == "None" or current == "" then return end

    local owned, ownedList = {}, {}
    local ok = pcall(function()
        gi.CharactersCollection:ForEach(function(k, v)
            local name = str(k)
            owned[name] = true
            ownedList[#ownedList + 1] = name
        end)
    end)
    if not ok or #ownedList == 0 then return end

    if owned[current] then
        lastReported = nil
        return
    end

    local replacement
    pcall(function()
        gi.CurrentParty:ForEach(function(i, e)
            if replacement then return end
            local name = str(unwrap(e)[SLOT_NAME])
            if owned[name] then replacement = name end
        end)
    end)
    replacement = replacement or ownedList[1]

    if lastReported ~= current then
        print(string.format("[LocationRandomizer] CurrentCharacterWorld '%s' not owned, overriding with '%s'\n",
            current, replacement))
        lastReported = current
    end
    gi.CurrentCharacterWorld = FName(replacement)
end

function M.start(pollMs)
    if started then return end
    started = true
    local interval = pollMs or POLL_MS
    LoopAsync(interval, function()
        ExecuteInGameThread(function()
            local ok, err = pcall(check_world_character)
            if not ok then print("[LocationRandomizer] CurrentCharacterWorld fix error: " .. tostring(err) .. "\n") end
        end)
        return false
    end)
    print("[LocationRandomizer] CurrentCharacterWorld fix started, polling every " .. interval .. " ms\n")
end

return M