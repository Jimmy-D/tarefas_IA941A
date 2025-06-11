using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Clarion;
using Clarion.Framework;
using Clarion.Framework.Core;
using Clarion.Framework.Templates;
using ClarionApp.Model;
using ClarionApp;
using System.Threading;
using Gtk;

namespace ClarionApp
{
	/// <summary>
	/// Public enum that represents all possibilities of agent actions
	/// </summary>
	public enum CreatureActions
	{
		ROTATE_CLOCKWISE,
		GO_TO_JEWEL,
		GO_TO_FOOD,
		GET_JEWEL,
		GET_FOOD,
		WANDER,
		GO_TO_DELIVER,
		DELIVER
	}

    public class ClarionAgent
    {
        #region Constants
        /// <summary>
        /// Constant that represents the Visual Sensor
        /// </summary>
        private String SENSOR_VISUAL_DIMENSION = "VisualSensor";
        /// <summary>
        /// Constant that represents that there is at least one wall ahead
        /// </summary>
        private String DIMENSION_WALL_AHEAD = "WallAhead";
		private String DIMENSION_JEWEL_AHEAD = "JewelAhead";
		private String DIMENSION_JEWEL_AWAY = "JewelAway";
		private String DIMENSION_FOOD_AHEAD = "FoodAhead";
		private String DIMENSION_FOOD_AWAY = "FoodAway";
		private String DIMENSION_ALL_JEWELS_COLLECTED = "AllJewelsCollected";
		private String DIMENSION_CREATURE_CAN_DELIVER = "CreatureCanDeliver";
		double prad = 0;

		private double CREATURE_CAN_DELIVER_ACT_VAL = 1.0;
		private double ALL_JEWELS_COLLECTED_ACT_VAL = 0.9;
		private double JEWEL_AHEAD_ACT_VAL = 0.8;
		private double JEWEL_AWAY_ACT_VAL = 0.5;
		private double FOOD_AHEAD_ACT_VAL = 0.7;
		private double FOOD_AWAY_ACT_VAL = 0.4;
		private double WALL_AHEAD_ACT_VAL = 0.6;
		private double MIN_ACT_VAL = 0.0;
		#endregion

		#region Properties
		public MindViewer mind;
		String creatureId = String.Empty;
		String creatureName = String.Empty;
		private Memory memory;
        #region Simulation
        /// <summary>
        /// If this value is greater than zero, the agent will have a finite number of cognitive cycle. Otherwise, it will have infinite cycles.
        /// </summary>
        public double MaxNumberOfCognitiveCycles = -1;
        /// <summary>
        /// Current cognitive cycle number
        /// </summary>
        private double CurrentCognitiveCycle = 0;
        /// <summary>
        /// Time between cognitive cycle in miliseconds
        /// </summary>
        public Int32 TimeBetweenCognitiveCycles = 0;
        /// <summary>
        /// A thread Class that will handle the simulation process
        /// </summary>
        private Thread runThread;
        #endregion

        #region Agent
		private WSProxy worldServer;
        /// <summary>
        /// The agent 
        /// </summary>
        private Clarion.Framework.Agent CurrentAgent;
        #endregion

        #region Perception Input
        /// <summary>
        /// Perception input to indicates a wall ahead
        /// </summary>
		private DimensionValuePair inputWallAhead;
		private DimensionValuePair inputJewelAhead;
		private DimensionValuePair inputFoodAhead;
		private DimensionValuePair inputJewelAway;
		private DimensionValuePair inputFoodAway;
		private DimensionValuePair inputAllJewelsCollected;
		private DimensionValuePair inputCreatureCanDeliver;
		#endregion

		#region Action Output
		/// <summary>
		/// Output action that makes the agent to rotate clockwise
		/// </summary>
		private ExternalActionChunk outputRotateClockwise;
		private ExternalActionChunk outputGetJewel;
		private ExternalActionChunk outputGetFood;
		private ExternalActionChunk outputGoToJewel;
		private ExternalActionChunk outputGoToFood;
		private ExternalActionChunk outputWander;
		private ExternalActionChunk outputGoToDeliverySpot;
		private ExternalActionChunk outputDoDelivery;

		#endregion

		#endregion

		#region Constructor
		public ClarionAgent(WSProxy nws, String creature_ID, String creature_Name)
        {
			worldServer = nws;
			// Initialize the agent
            CurrentAgent = World.NewAgent("Current Agent");
			mind = new MindViewer();
			mind.Show ();
			creatureId = creature_ID;
			creatureName = creature_Name;

			memory = new Memory ();

            // Initialize Input Information
            inputWallAhead = World.NewDimensionValuePair(SENSOR_VISUAL_DIMENSION, DIMENSION_WALL_AHEAD);
			inputJewelAhead = World.NewDimensionValuePair (SENSOR_VISUAL_DIMENSION, DIMENSION_JEWEL_AHEAD);
			inputFoodAhead = World.NewDimensionValuePair (SENSOR_VISUAL_DIMENSION, DIMENSION_FOOD_AHEAD);
			inputJewelAway = World.NewDimensionValuePair (SENSOR_VISUAL_DIMENSION, DIMENSION_JEWEL_AWAY);
			inputFoodAway = World.NewDimensionValuePair (SENSOR_VISUAL_DIMENSION, DIMENSION_FOOD_AWAY);
			inputAllJewelsCollected = World.NewDimensionValuePair (SENSOR_VISUAL_DIMENSION, DIMENSION_ALL_JEWELS_COLLECTED);
			inputCreatureCanDeliver = World.NewDimensionValuePair (SENSOR_VISUAL_DIMENSION, DIMENSION_CREATURE_CAN_DELIVER);

			// Initialize Output actions
			outputRotateClockwise = World.NewExternalActionChunk(CreatureActions.ROTATE_CLOCKWISE.ToString());
			outputGetJewel = World.NewExternalActionChunk (CreatureActions.GET_JEWEL.ToString ());
			outputGetFood = World.NewExternalActionChunk (CreatureActions.GET_FOOD.ToString ());
			outputGoToJewel = World.NewExternalActionChunk (CreatureActions.GO_TO_JEWEL.ToString ());
			outputGoToFood = World.NewExternalActionChunk (CreatureActions.GO_TO_FOOD.ToString ());
			outputWander = World.NewExternalActionChunk (CreatureActions.WANDER.ToString ());
			outputGoToDeliverySpot = World.NewExternalActionChunk (CreatureActions.GO_TO_DELIVER.ToString ());
			outputDoDelivery = World.NewExternalActionChunk (CreatureActions.DELIVER.ToString ());

			//Create thread to simulation
			runThread = new Thread(CognitiveCycle);
			Console.WriteLine("Agent started");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Run the Simulation in World Server 3d Environment
        /// </summary>
        public void Run()
        {                
			Console.WriteLine ("Running ...");
            // Setup Agent to run
            if (runThread != null && !runThread.IsAlive)
            {
                SetupAgentInfraStructure();
				// Start Simulation Thread                
                runThread.Start(null);
            }
        }

        /// <summary>
        /// Abort the current Simulation
        /// </summary>
        /// <param name="deleteAgent">If true beyond abort the current simulation it will die the agent.</param>
        public void Abort(Boolean deleteAgent)
        {   Console.WriteLine ("Aborting ...");
            if (runThread != null && runThread.IsAlive)
            {
                runThread.Abort();
            }

            if (CurrentAgent != null && deleteAgent)
            {
                CurrentAgent.Die();
            }
        }

		IList<Thing> processSensoryInformation()
		{
			IList<Thing> response = null;

			if (worldServer != null && worldServer.IsConnected)
			{
				response = worldServer.SendGetCreatureState(creatureName);
				prad = (Math.PI / 180) * response.First().Pitch;
				while (prad > Math.PI) prad -= 2 * Math.PI;
				while (prad < - Math.PI) prad += 2 * Math.PI;
				Sack s = worldServer.SendGetSack("0");
				mind.setBag(s);
			}

			return response;
		}

		void processSelectedAction(CreatureActions externalAction)
		{   Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			if (worldServer != null && worldServer.IsConnected)
			{
				Console.WriteLine ("\n Action: " + externalAction);
				switch (externalAction)
				{
				case CreatureActions.ROTATE_CLOCKWISE:
					worldServer.SendSetAngle(creatureId, 2, -2, 2);
					break;
				case CreatureActions.GET_JEWEL:
					Thing jewelToGet = memory.getNearestJewel ();
					worldServer.SendSackIt (creatureId, jewelToGet.Name);
					memory.Remove (jewelToGet);
					break;
				case CreatureActions.GET_FOOD:
					Thing foodToGet = memory.getNearestFood ();
					worldServer.SendEatIt (creatureId, foodToGet.Name);
					memory.Remove (foodToGet);
					break;
				case CreatureActions.GO_TO_JEWEL:
					Thing jewelToGoTo = memory.getNearestJewel ();
					worldServer.SendSetAngle (creatureId, 0, 0, prad);
					worldServer.SendSetGoTo (creatureId, 2, 2, jewelToGoTo.X1, jewelToGoTo.Y1);
					break;
				case CreatureActions.GO_TO_FOOD:
					Thing foodToGoTo = memory.getNearestFood ();
					worldServer.SendSetAngle (creatureId, 0, 0, prad);
					worldServer.SendSetGoTo (creatureId, 2, 2, foodToGoTo.X1, foodToGoTo.Y1);
					break;
				case CreatureActions.WANDER:
					worldServer.SendSetAngle (creatureId, 2, -2, 2);
					break;
				case CreatureActions.GO_TO_DELIVER:
					// Send creature to the delivery spot.
					worldServer.SendSetAngle (creatureId, 0, 0, prad);
					worldServer.SendSetGoTo (creatureId, 2, 2, memory.deliverySpot.X1, memory.deliverySpot.Y1);
					break;
				case CreatureActions.DELIVER:
					IList<Leaflet> leaflets = memory.creature.getLeaflets ();
					worldServer.SendDeliver (creatureId, leaflets[0].leafletID.ToString());
					worldServer.SendDeliver (creatureId, leaflets [1].leafletID.ToString ());
					worldServer.SendDeliver (creatureId, leaflets [2].leafletID.ToString ());
					worldServer.SendStopCreature (creatureId);
					break;
				default:
					break;
				}
			}
		}

        #endregion

        #region Setup Agent Methods
        /// <summary>
        /// Setup agent infra structure (ACS, NACS, MS and MCS)
        /// </summary>
        private void SetupAgentInfraStructure()
        {
            // Setup the ACS Subsystem
            SetupACS();                    
        }

        private void SetupMS()
        {            
            //RichDrive
        }

        /// <summary>
        /// Setup the ACS subsystem
        /// </summary>
        private void SetupACS()
        {
            // Create Rule to avoid collision with wall
            SupportCalculator avoidCollisionWallSupportCalculator = FixedRuleToAvoidCollisionWall;
            FixedRule ruleAvoidCollisionWall = AgentInitializer.InitializeActionRule(CurrentAgent, FixedRule.Factory, outputRotateClockwise, avoidCollisionWallSupportCalculator);

            // Commit this rule to Agent (in the ACS)
            CurrentAgent.Commit(ruleAvoidCollisionWall);

			SupportCalculator wanderSupportCalculator = FixedRuleToWander;
			FixedRule ruleWander = AgentInitializer.InitializeActionRule (CurrentAgent, FixedRule.Factory, outputWander, wanderSupportCalculator);
			CurrentAgent.Commit(ruleWander);

			SupportCalculator getJewelSupportCalculator = FixedRuleToGetJewel;
			FixedRule ruleGetJewel = AgentInitializer.InitializeActionRule (CurrentAgent, FixedRule.Factory, outputGetJewel, getJewelSupportCalculator);
			CurrentAgent.Commit (ruleGetJewel);

			SupportCalculator getFoodSupportCalculator = FixedRuleToGetFood;
			FixedRule ruleGetFood = AgentInitializer.InitializeActionRule (CurrentAgent, FixedRule.Factory, outputGetFood, getFoodSupportCalculator);
			CurrentAgent.Commit (ruleGetFood);

			SupportCalculator goToJewelSupportCalculator = FixedRuleToGoToJewel;
			FixedRule ruleGoToJewel = AgentInitializer.InitializeActionRule (CurrentAgent, FixedRule.Factory, outputGoToJewel, goToJewelSupportCalculator);
			CurrentAgent.Commit (ruleGoToJewel);

			SupportCalculator goToFoodSupportCalculator = FixedRuleToGoToFood;
			FixedRule ruleGoToFood = AgentInitializer.InitializeActionRule (CurrentAgent, FixedRule.Factory, outputGoToFood, goToFoodSupportCalculator);
			CurrentAgent.Commit (ruleGoToFood);

			SupportCalculator goToDeliverySpotSupportCalculator = FixedRuleToGoToDeliverySpot;
			FixedRule ruleGoToDeliverySpot = AgentInitializer.InitializeActionRule (CurrentAgent, FixedRule.Factory, outputGoToDeliverySpot,
																				   goToDeliverySpotSupportCalculator);
			CurrentAgent.Commit (ruleGoToDeliverySpot);

			SupportCalculator deliverSupportCalculator = FixedRuleToDeliver;
			FixedRule ruleDeliver = AgentInitializer.InitializeActionRule (CurrentAgent, FixedRule.Factory, outputDoDelivery,
				deliverSupportCalculator);
			CurrentAgent.Commit (ruleDeliver);

			// Disable Rule Refinement
			CurrentAgent.ACS.Parameters.PERFORM_RER_REFINEMENT = false;

            // The selection type will be probabilistic
            CurrentAgent.ACS.Parameters.LEVEL_SELECTION_METHOD = ActionCenteredSubsystem.LevelSelectionMethods.STOCHASTIC;

            // The action selection will be fixed (not variable) i.e. only the statement defined above.
            CurrentAgent.ACS.Parameters.LEVEL_SELECTION_OPTION = ActionCenteredSubsystem.LevelSelectionOptions.FIXED;

            // Define Probabilistic values
            CurrentAgent.ACS.Parameters.FIXED_FR_LEVEL_SELECTION_MEASURE = 1;
            CurrentAgent.ACS.Parameters.FIXED_IRL_LEVEL_SELECTION_MEASURE = 0;
            CurrentAgent.ACS.Parameters.FIXED_BL_LEVEL_SELECTION_MEASURE = 0;
            CurrentAgent.ACS.Parameters.FIXED_RER_LEVEL_SELECTION_MEASURE = 0;
        }

		/// <summary>
		/// Make the agent perception. In other words, translate the information that came from sensors to a new type that the agent can understand
		/// </summary>
		/// <param name="sensorialInformation">The information that came from server</param>
		/// <returns>The perceived information</returns>
		private SensoryInformation prepareSensoryInformation (IList<Thing> listOfThings)
		{
			// New sensory information
			SensoryInformation si = World.NewSensoryInformation (CurrentAgent);

			// Detect if we have a wall ahead
			//Boolean wallAhead = listOfThings.Where(item => (item.CategoryId == Thing.CATEGORY_BRICK && item.DistanceToCreature <= 61)).Any();
			//double wallAheadActivationValue = wallAhead ? CurrentAgent.Parameters.MAX_ACTIVATION : CurrentAgent.Parameters.MIN_ACTIVATION;
			//si.Add(inputWallAhead, wallAheadActivationValue);
			//Console.WriteLine(sensorialInformation);
			Creature c = (Creature)listOfThings.Where (item => (item.CategoryId == Thing.CATEGORY_CREATURE)).First ();
			memory.creature = c;
			int n = 0;
			foreach (Leaflet l in c.getLeaflets ()) {
				mind.updateLeaflet (n, l);
				n++;
			}

			double wallAheadActivationValue = MIN_ACT_VAL;
			double jewelAheadActivationValue = MIN_ACT_VAL;
			double foodAheadActivationValue = MIN_ACT_VAL;
			double jewelAwayActivationValue = MIN_ACT_VAL;
			double foodAwayActivationValue = MIN_ACT_VAL;
			double allJewelsCollectedActivationValue = MIN_ACT_VAL;
			double creatureCanDeliverActivationValue = MIN_ACT_VAL;

			bool actionSelected = false;

			Console.WriteLine ("Activations:");
			if (!actionSelected) {
				if (memory.deliverySpot != null &&
					memory.deliverySpot.DistanceToCreature <= 50 &&
					c.getLeaflets ().Where ((l) => l.situation == true).Any ()) {
					Console.WriteLine ("\tActivation Deliver");
					creatureCanDeliverActivationValue = CREATURE_CAN_DELIVER_ACT_VAL;
					actionSelected = true;
				}
			}

			if (!actionSelected) {
				foreach (Thing jewel in memory.GetJewels ()) {
					if (jewel.DistanceToCreature <= 50) {
						Console.WriteLine ("\tActivation get Jewel");
						jewelAheadActivationValue = JEWEL_AHEAD_ACT_VAL;
						actionSelected = true;
					}
				}
			}

			if (!actionSelected) {
				foreach (Thing food in memory.GetFoods ()) {
					if (food.DistanceToCreature <= 50) {
						Console.WriteLine ("\tActivation get Food");
						foodAheadActivationValue = FOOD_AHEAD_ACT_VAL;
						actionSelected = true;
					}
				}
			}

			if (!actionSelected) {
				if (listOfThings.Where (item => (item.CategoryId == Thing.CATEGORY_BRICK && item.DistanceToCreature <= 30)).Any ()) {
					Console.WriteLine ("\tActivation Avoid Brick");
					wallAheadActivationValue = WALL_AHEAD_ACT_VAL;
					actionSelected = true;
				}
			}

			if (!actionSelected) {
				if (memory.GetFoods ().Any () && c.Fuel < 400) {
					Console.WriteLine ("\tActivation go Food");
					foodAwayActivationValue = FOOD_AWAY_ACT_VAL;
					actionSelected = true;
				}
			}

			if (!actionSelected) {
				if (memory.deliverySpot != null && c.getLeaflets ().Where ((l) => l.situation == true).Any ()) {
					Console.WriteLine ("\tActivation go Delivery");
					allJewelsCollectedActivationValue = ALL_JEWELS_COLLECTED_ACT_VAL;
					actionSelected = true;
				}
			}

			if (!actionSelected) {
				if (memory.GetJewels ().Any ()) {
					Console.WriteLine ("\tActivation go Jewel");
					jewelAwayActivationValue = JEWEL_AWAY_ACT_VAL;
					actionSelected = true;
				}
			}

			si.Add (inputWallAhead, wallAheadActivationValue);
			si.Add (inputJewelAhead, jewelAheadActivationValue);
			si.Add (inputFoodAhead, foodAheadActivationValue);
			si.Add (inputJewelAway, jewelAwayActivationValue);
			si.Add (inputFoodAway, foodAwayActivationValue);
			si.Add (inputAllJewelsCollected, allJewelsCollectedActivationValue);
			si.Add (inputCreatureCanDeliver, creatureCanDeliverActivationValue);

			return si;
        }
        #endregion

        #region Fixed Rules
        private double FixedRuleToAvoidCollisionWall(ActivationCollection currentInput, Rule target)
        {
            // See partial match threshold to verify what are the rules available for action selection
            return ((currentInput.Contains(inputWallAhead, WALL_AHEAD_ACT_VAL))) ? 1.0 : 0.0;
        }

		private double FixedRuleToWander (ActivationCollection currentInput, Rule target)
		{
			// Here we will make the logic to wander - check for low activation
			// in all inputs.
			if (currentInput.Contains (inputAllJewelsCollected, MIN_ACT_VAL) &&
				currentInput.Contains (inputCreatureCanDeliver, MIN_ACT_VAL) &&
				currentInput.Contains (inputWallAhead, MIN_ACT_VAL) &&
				currentInput.Contains (inputJewelAhead, MIN_ACT_VAL) &&
				currentInput.Contains (inputFoodAhead, MIN_ACT_VAL) &&
				currentInput.Contains (inputJewelAway, MIN_ACT_VAL) &&
				currentInput.Contains (inputFoodAway, MIN_ACT_VAL)) {
				return 1.0;
			} else {
				return 0.0;
			}
		}

		private double FixedRuleToGoToDeliverySpot (ActivationCollection currentInput, Rule target)
		{
			// Check if all jewels collected.
			return ((currentInput.Contains (inputAllJewelsCollected, ALL_JEWELS_COLLECTED_ACT_VAL))) ? 1.0 : 0.0;
		}

		private double FixedRuleToDeliver (ActivationCollection currentInput, Rule target)
		{
			return ((currentInput.Contains (inputCreatureCanDeliver, CREATURE_CAN_DELIVER_ACT_VAL))) ? 1.0 : 0.0;
		}

		private double FixedRuleToGetJewel (ActivationCollection currentInput, Rule target)
		{
			// Here we will make the logic to collect a jewel
			return ((currentInput.Contains (inputJewelAhead, JEWEL_AHEAD_ACT_VAL))) ? 1.0 : 0.0;
		}

		private double FixedRuleToGetFood (ActivationCollection currentInput, Rule target)
		{
			// Here we will make the logic to eat food
			return ((currentInput.Contains (inputFoodAhead, FOOD_AHEAD_ACT_VAL))) ? 1.0 : 0.0;
		}

		private double FixedRuleToGoToJewel (ActivationCollection currentInput, Rule target)
		{
			// Here we will make the logic to collect a jewel
			return ((currentInput.Contains (inputJewelAway, JEWEL_AWAY_ACT_VAL))) ? 1.0 : 0.0;
		}

		private double FixedRuleToGoToFood (ActivationCollection currentInput, Rule target)
		{
			// Here we will make the logic to eat food
			return ((currentInput.Contains (inputFoodAway, FOOD_AWAY_ACT_VAL))) ? 1.0 : 0.0;
		}
		#endregion

		#region Run Thread Method
		private void CognitiveCycle(object obj)
        {

			Console.WriteLine("Starting Cognitive Cycle ... press CTRL-C to finish !");
			// Cognitive Cycle starts here getting sensorial information
			while (CurrentCognitiveCycle != MaxNumberOfCognitiveCycles) {
				// Get current sensory information                    
				IList<Thing> currentSceneInWS3D = processSensoryInformation ();
				memory.Update (currentSceneInWS3D);

				// Make the perception
				SensoryInformation si = prepareSensoryInformation(currentSceneInWS3D);

                //Perceive the sensory information
                CurrentAgent.Perceive(si);

                //Choose an action
                ExternalActionChunk chosen = CurrentAgent.GetChosenExternalAction(si);

                // Get the selected action
                String actionLabel = chosen.LabelAsIComparable.ToString();
				Console.WriteLine ("Action label: " + actionLabel);
                CreatureActions actionType = (CreatureActions)Enum.Parse(typeof(CreatureActions), actionLabel, true);

                // Call the output event handler
				processSelectedAction(actionType);

                // Increment the number of cognitive cycles
                CurrentCognitiveCycle++;

                //Wait to the agent accomplish his job
                if (TimeBetweenCognitiveCycles > 0)
                {
                    Thread.Sleep(TimeBetweenCognitiveCycles);
                }
			}
        }
        #endregion

    }
}
