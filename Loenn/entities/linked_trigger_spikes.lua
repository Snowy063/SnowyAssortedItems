local spikeHelper = require("helpers.spikes")

local spikeVariants = {
    "default",
    "outline",
    "cliffside",
    "reflection",
}

local spikeUp = spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesUp", "up", false, true, spikeVariants)
local spikeDown = spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesDown", "down", false, true, spikeVariants)
local spikeLeft = spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesLeft", "left", false, true, spikeVariants)
local spikeRight = spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesRight", "right", false, true, spikeVariants)

local allSpikes = { spikeUp, spikeDown, spikeLeft, spikeRight }

local regularSpikes = {
    spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesUp", "up", false, true),
    spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesDown", "down", false, true),
    spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesLeft", "left", false, true),
    spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesRight", "right", false, true)
}

local dustSpikes = {
    spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesUp", "up", true),
    spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesDown", "down", true),
    spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesLeft", "left", true),
    spikeHelper.createEntityHandler("SnowyAssortedItems/LinkedTriggerSpikesRight", "right", true)
}

-- pick between the dust handler and the regular handler depending on the entity type
for i=1,4,1 do
    local spike = allSpikes[i]
    local regularSpike = regularSpikes[i]
    local dustSpike = dustSpikes[i]

    function spike.sprite(room, entity)
        if entity.type == "dust" then
            return dustSpike.sprite(room, entity)
        else
            return regularSpike.sprite(room, entity)
        end
    end

    function spike.selection(room, entity)
        if entity.type == "dust" then
            return dustSpike.selection(room, entity)
        else
            return regularSpike.selection(room, entity)
        end
    end
end

for key, value in ipairs(allSpikes) do
    for key2, placement in ipairs(value.placements) do
        placement.data.flag = ""
        placement.data.persistent = false
    end
end

return {
    spikeUp,
    spikeDown,
    spikeLeft,
    spikeRight
}