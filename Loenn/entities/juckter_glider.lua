local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local juckterGlider = {}

juckterGlider.name = "SnowyAssortedItems/JuckterGlider"
juckterGlider.depth = -5
juckterGlider.placements = {
    {
        name = "juckter_jellyfish",
        data = {
            bubble = false
        }
    },
    {
        name = "juckter_jellyfish_floating",
        data = {
            bubble = true
        }
    }
}

local texture = "objects/glider/idle0"

function juckterGlider.sprite(room, entity)
    local bubble = entity.bubble

    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y - 1}, {x + 11, y - 1}, {x - 0, y - 6})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()
        local jellySprite = drawableSprite.fromTexture(texture, entity)

        table.insert(lineSprites, 1, jellySprite)

        return lineSprites

    else
        return drawableSprite.fromTexture(texture, entity)
    end
end

function juckterGlider.rectangle(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    return sprite:getRectangle()
end

return juckterGlider