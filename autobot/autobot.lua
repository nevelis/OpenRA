-- This is a test

state = {
	bases = 0,
	building = nil
}

function OnInit()
	log('Autobot Script Starting YEOWH')
end

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

function pickNextBuilding()
	state['building'] = nil

	local p = GetPowerUsage()
	if p <= 0 then
		log('Need power - building power plant')
	end

	if state['building'] ~= nil then
		Build(state['building'])
	end
end

