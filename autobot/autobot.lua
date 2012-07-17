--
-- Demonstrates a basic AI script to lead an OpenRA team to victory!
--

-- Some global state for our bot
state = {
	bases = 0,     -- How many bases we currently have
	building = nil -- Which building we are constructing, nil if none
}

-- Called when '/run' is typed in the chat box
function OnInit()
	log('Autobot Script Starting YEOWH')
end

-- After OnInit() is called, OnThink() will be called periodically
function OnThink()
	log('Thinking...')

	if state['bases'] == 0 then
		log('No base! Finding MCV...')

		local mcv = FindUnitByName('mcv')
		if mcv == nil then
			log('PANIC: NO MCV!')
			return
		end

		log('Found unit: ', mcv['name'], ', id: ', mcv['id'])

		-- Deploy it if we can
		log('Deploying unit...')
		DeployUnit(mcv)
	end
end

-- Called when a unit is deployed. Parameter is the new unit/building
function OnUnitDeployed(unit)
	log('Unit deployed: ', unit['name'], ', id: ', unit['id'])

	if unit['name'] == 'fact' then
		state['bases'] = state['bases'] + 1
		log('MCV deployed, base operational!')

		if state['building'] ~= nil then
			pickNextBuilding()
		end
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

-- User function: Does some sanity checking & chooses the next
-- building to build.
function pickNextBuilding()
	state['building'] = nil

	local p = GetPowerUsage()
	if p <= 0 then
		log('Need power - building power plant')
		state['building'] = 'powr'
	end

	if state['building'] ~= nil then
		Build(state['building'])
	end
end

