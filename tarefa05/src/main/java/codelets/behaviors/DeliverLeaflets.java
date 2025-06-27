/*****************************************************************************
 * Copyright 2007-2015 DCA-FEEC-UNICAMP
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * Contributors:
 *    Klaus Raizer, Andre Paraense, Ricardo Ribeiro Gudwin
 *****************************************************************************/

package codelets.behaviors;

import java.awt.Point;
import java.awt.geom.Point2D;

import org.json.JSONException;
import org.json.JSONObject;

import br.unicamp.cst.core.entities.Codelet;
import br.unicamp.cst.core.entities.Memory;
import br.unicamp.cst.core.entities.MemoryObject;
import br.unicamp.cst.representation.idea.Idea;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.logging.Level;
import java.util.logging.Logger;
import ws3dproxy.model.Creature;
import ws3dproxy.model.Leaflet;
import ws3dproxy.model.Thing;

public class DeliverLeaflets extends Codelet {

	private Memory innerSenseMO;
        private Memory knownMO;
	private int reachDistance;
        Idea cis;
        Creature c;

	public DeliverLeaflets(int reachDistance, Creature nc) {
                setTimeStep(50);
		this.reachDistance=reachDistance;
                this.name = "DeliverLeaflets";
                this.c = nc;
	}

	@Override
	public void accessMemoryObjects() {
		innerSenseMO=(MemoryObject)this.getInput("INNER");
	}

	@Override
	public void proc() {
                String jewelName="";
                cis = (Idea) innerSenseMO.getI();
                double completed = (double) cis.get("completed").getValue();

		
		if(completed == 1)
		{
			double selfX=(double)cis.get("position.x").getValue();
			double selfY=(double)cis.get("position.y").getValue();

			Point2D pDelivery = new Point();
			pDelivery.setLocation(400, 300);

			Point2D pSelf = new Point();
			pSelf.setLocation(selfX, selfY);

			double distance = pSelf.distance(pDelivery);
			try {
				if(distance<=reachDistance){ //eat it						
                                    try {
                                        List<Leaflet> leaflets = c.getLeaflets();
                                        for (Leaflet l : leaflets) {
                                            if(l.getSituation() == 1) 
                                                c.deliverLeaflet(l.getID().toString());
                                        }
                                    } catch (Exception e) {}                                         
				}
				
//				System.out.println(message);
			} catch (JSONException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
        //System.out.println("Before: "+known.size()+ " "+known);
        
        //System.out.println("After: "+known.size()+ " "+known);
	//System.out.println("EatClosestApple: "+ handsMO.getInfo());	

	}
        
        @Override
        public void calculateActivation() {
        
        }
        

}
