--
-- Demonstrates a basic AI script to lead an OpenRA team to victory!
--

-- Some global state for our bot
state = {
	relocating_mcv = false,
	building = nil, -- Which building we are constructing, nil if none
	infantry = nil  -- Which unit we're building, nil if none
}

barracks_name = ''
army = {}

-- Called when '/run' is typed in the chat box
function OnInit()
	log('Autobot Script Starting...')
	-- log('Team: ', Team())

	-- Set team-specific buildings & army sizes
	if Team() == 'allies' then
		barracks_name = 'tent'
		army['e1'] = 7
		army['e3'] = 4
	else
		barracks_name = 'barr'
		army['e1'] = 7
		army['e3'] = 4
	end
end

-- After OnInit() is called, OnThink() will be called periodically
function OnThink()
	log('Thinking...')

	log('CanBuild("powr"): ', CanBuild('powr'))
-- 
-- 	-- If we don't have a construction yard deployed
-- 	local x = GetBuildingCount('fact')
-- 	log('Found ', x, ' buildings matching "fact"')
-- 
-- 	if x == 0 then
-- 		-- Lets not deply the MCV if we're intentionally moving it...
-- 		if state['relocating_mcv'] == false then
-- 			log('No base! Finding MCV...')
-- 
-- 			local mcv = FindUnitByName('mcv')
-- 			if mcv == nil then
-- 				log('PANIC: NO MCV!')
-- 				return
-- 			end
-- 
-- 			log('Found unit: ', mcv['name'], ', id: ', mcv['id'])
-- 
-- 			-- Deploy it if we can
-- 			log('Deploying unit...')
-- 			DeployUnit(mcv)
-- 		end
-- 	end
-- 
-- 	pickNextBuilding()
-- 	pickNextInfantry()
end

-- Called when a unit is deployed. Parameter is the new unit/building
function OnUnitDeployed(unit)
	log('Unit deployed: ', unit['name'], ', id: ', unit['id'])

	if unit['name'] == 'fact' then
		log('MCV deployed, base operational!')
	end
end

-- Called when one of the players units gets attacked by an enemy
function OnUnitAttacked(unit, enemy)
	-- If we have a chance of killing the enemy, try it
	if IsEffectiveAgainst(unit, enemy) then
		Attack(unit, enemy)
	else
		-- Otherwise run away
		Retreat(unit, GetBaseLocation(), 3)
	end

	-- Do we have friendlies nearby that can help?
	local friendlies = GetNearbyUnits(unit)
	for k, v in pairs(friendlies) do
		if IsEffectiveAgainst(v, enemy) then
			Attack(v, enemy)
			break
		end
	end
end

function OnConstructionComplete(name)
	if name == 'powr' or name == 'apwr' then
		-- 'inner' will try to place the building behind defenses
		DeployBuilding(name, 'inner')
	elseif name == 'proc' then
		-- 'ore' will try to place the building near ore
		DeployBuilding(name, 'ore')
	end
end

function OnUnitReady(unit)
	log('Unit ready: ', unit['name'])
	pickNextInfantry()
end

-- User function: Starts building something
function build(name)
	state['building'] = name
	Build(name)

	return 0
end

function idealRefineries()
	-- TODO: Pick an ideal number of refineries based on army size & cash monies
	return 1
end

-- User function: Does some sanity checking & chooses the next
-- building to build.
function pickNextBuilding()
	-- Don't start building something else if we are already
	if state['building'] ~= nil then
		return
	end

	-- Check our power level
	local p = GetPowerExcess()
    log('Power excess: ', p)
	if p <= 0 then
		log('Need power (have ', p, ') - building power plant')

		-- Try building an advanced power plant first
		if CanBuild('apwr') then
			return build('apwr')
		end

		return build('powr')
	end

	-- Do we have refineries?
	local c = GetBuildingCount('proc')
	if c < idealRefineries() then
		return build('proc')
	end

	-- How about barracks/tent?
	if GetBuildingCount(barracks_name) < 1 then
		return build(barracks_name)
	end
end

function pickNextInfantry()
	if state['infantry'] ~= nil then
		return
	end

	if GetBuildingCount(barracks_name) < 1 then
		-- Don't have a barracks!
		return
	end

	local all_units = GetInfantry()

	for k, v in pairs(army) do
		local amount = CountUnits(k, all_units)

		if amount < v then
			log('Unit type ', k, '\'s count of ', amount, ' is less than target ', v)
			state['infantry'] = k
			Build(k)
			return
		end
	end
end

