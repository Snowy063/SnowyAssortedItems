local drawableSprite = require("structs.drawable_sprite")

local juckterTheoCrystal = {}

juckterTheoCrystal.name = "SnowyAssortedItems/JuckterTheoCrystal"
juckterTheoCrystal.depth = 100
juckterTheoCrystal.placements = {
    name = "juckter_theo_crystal",
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "characters/theoCrystal/idle00"

function juckterTheoCrystal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end

return juckterTheoCrystal