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

package codelets.perception;



import br.unicamp.cst.core.entities.Codelet;
import br.unicamp.cst.core.entities.Memory;
import br.unicamp.cst.core.entities.MemoryObject;
import br.unicamp.cst.representation.idea.Idea;
import java.util.Collections;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;
import ws3dproxy.model.Thing;

/**
 * @author klaus
 *
 */
public class ClosestJewelDetector extends Codelet {

	private Memory knownMO;
	private Memory closestJewelMO;
	private Memory innerSenseMO;
	
        private List<Thing> known;

	public ClosestJewelDetector() {
            this.name = "ClosestJewelDetector";
	}


	@Override
	public void accessMemoryObjects() {
		this.knownMO=(MemoryObject)this.getInput("KNOWN_JEWELS");
		this.innerSenseMO=(MemoryObject)this.getInput("INNER");
		this.closestJewelMO=(MemoryObject)this.getOutput("CLOSEST_JEWEL");	
	}
	@Override
	public void proc() {
                Thing closest_jewel=null;
                known = Collections.synchronizedList((List<Thing>) knownMO.getI());
                Idea cis = (Idea) innerSenseMO.getI();
                synchronized(known) {
		   if(known.size() != 0){
                        CopyOnWriteArrayList<Thing> myknown = new CopyOnWriteArrayList<>(known);
                        for (Thing t : myknown) {
				String objectName=t.getName();
				if(objectName.contains("Jewel")){ 
                                        if(closest_jewel == null){    
                                                closest_jewel = t;
					}
                                        else {
						double Dnew = calculateDistance(t.getX1(), t.getY1(), (double)cis.get("position.x").getValue(), (double)cis.get("position.y").getValue());
                                                double Dclosest= calculateDistance(closest_jewel.getX1(), closest_jewel.getY1(), (double)cis.get("position.x").getValue(), (double)cis.get("position.y").getValue());
						if(Dnew<Dclosest){
                                                        closest_jewel = t;
						}
					}
				}
			}
		   }
                }
//                if (closest_apple != null)
//                   System.out.println("Closest apple: "+closest_apple.getName()+" known: "+known.size());
//                else
//                   System.out.println("Closest apple: null"+" known: "+known.size()); 
                closestJewelMO.setI(closest_jewel);
	}//end proc

@Override
        public void calculateActivation() {
        
        }
        
        private double calculateDistance(double x1, double y1, double x2, double y2) {
            return(Math.sqrt(Math.pow(x1-x2, 2)+Math.pow(y1-y2, 2)));
        }

}
