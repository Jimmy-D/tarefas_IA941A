using System.Collections.Generic;
using System.Linq;
using ClarionApp.Model;

namespace ClarionApp
{
	public class Memory
	{
		public IList<Thing> memoryJewel = new List<Thing> ();
		public IList<Thing> memoryFood = new List<Thing> ();
		public Creature creature { get; set; }
		public Thing deliverySpot { get; set; }

		public void Update (IList<Thing> currentSceneInWS3D)
		{
			UpdateMemoryJewel (currentSceneInWS3D);
			UpdateMemoryFood (currentSceneInWS3D);
			foreach (var thing in currentSceneInWS3D) {
				if (thing.isFood()) {
					if (!memoryFood.Any (t => t.Name == thing.Name)) {
						memoryFood.Add (thing);
					}
				} else if (thing.isJewel()) {
					if (!memoryJewel.Any (t => t.Name == thing.Name)) {
						memoryJewel.Add (thing);
					}
				} else if (thing.isDeliverySpot ()) {
					deliverySpot = thing;
				}
			}
		}

		private void UpdateMemoryJewel (IList<Thing> listThings)
		{
			if (listThings == null) {
				return;
			}
			for (int i = 0; i < memoryJewel.Count; i++) {
				var newItem = listThings.FirstOrDefault (t => t.Name == memoryJewel [i].Name);
				if (newItem != null) {
					memoryJewel [i] = newItem;
				}
			}
		}

		private void UpdateMemoryFood (IList<Thing> listThings)
		{
			if (listThings == null) {
				return;
			}
			for (int i = 0; i < memoryFood.Count; i++) {
				var newItem = listThings.FirstOrDefault (t => t.Name == memoryFood [i].Name);
				if (newItem != null) {
					memoryFood [i] = newItem;
				}
			}
		}

		public IList<Thing> GetJewels ()
		{
			return memoryJewel;
		}

		public IList<Thing> GetFoods ()
		{
			return memoryFood;
		}

		public IList<Thing> GetFoodsJewels ()
		{
			List<Thing> all = new List<Thing> ();
			all.AddRange (memoryJewel);
			all.AddRange (memoryFood);
			return all;
		}

		public void Remove (Thing thing_to_remove)
		{
			if (thing_to_remove.isJewel()) {
				memoryJewel.Remove (thing_to_remove);
			}
			if (thing_to_remove.isFood()) {
				memoryFood.Remove (thing_to_remove);
			}
		}

		private Thing getNearest(IList<Thing> things)
		{
			Thing nearest = null;
			double minDistance = double.MaxValue;

			foreach(Thing thing in things) {
				double distance = thing.DistanceToCreature;

				if(distance < minDistance) {
					minDistance = distance;
					nearest = thing;
				}
			}

			return nearest;
		}

		public Thing getNearestFood ()
		{
			return getNearest (memoryFood);
		}

		public Thing getNearestJewel ()
		{
			return getNearest (memoryJewel);
		}
	}
}
