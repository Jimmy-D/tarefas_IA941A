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
import br.unicamp.cst.core.entities.Codelet;
import br.unicamp.cst.core.entities.Memory;
import br.unicamp.cst.core.entities.MemoryContainer;
import br.unicamp.cst.core.entities.MemoryObject;
import br.unicamp.cst.representation.idea.Idea;
import ws3dproxy.model.Thing;

public class GoToClosestJewel extends Codelet {

	private Memory closestJewelMO;
	private Memory selfInfoMO;
	private MemoryContainer legsMO;
	private int creatureBasicSpeed;
	private double reachDistance;

	public GoToClosestJewel(int creatureBasicSpeed, int reachDistance) {
		this.creatureBasicSpeed=creatureBasicSpeed;
		this.reachDistance=reachDistance;
                this.name = "GoToClosestJewel";
	}

	@Override
	public void accessMemoryObjects() {
		closestJewelMO=(MemoryObject)this.getInput("CLOSEST_JEWEL");
		selfInfoMO=(MemoryObject)this.getInput("INNER");
		legsMO=(MemoryContainer)this.getOutput("LEGS");
	}

	@Override
	public void proc() {

                Thing closestJewel = (Thing) closestJewelMO.getI();
                Idea cis = (Idea) selfInfoMO.getI();

		if(closestJewel != null)
		{
			double jewelX=0;
			double jewelY=0;
			try {
                                jewelX = closestJewel.getCenterPosition().getX();
                                jewelY = closestJewel.getCenterPosition().getY();

			} catch (Exception e) {
				e.printStackTrace();
			}
                        
			double selfX=(double)cis.get("position.x").getValue();
			double selfY=(double)cis.get("position.y").getValue();

			Point2D pJewel = new Point();
			pJewel.setLocation(jewelX, jewelY);

			Point2D pSelf = new Point();
			pSelf.setLocation(selfX, selfY);

			double distance = pSelf.distance(pJewel);
			//JSONObject message=new JSONObject();
                        Idea message = Idea.createIdea("goToClosestJewel","", Idea.guessType("Property",null,1.0,0.5));
			try {
				if(distance>reachDistance){ //Go to it
                                        message.add(Idea.createIdea("ACTION","GOTO", Idea.guessType("Property",null,1.0,0.5)));
                                        message.add(Idea.createIdea("X",(int)jewelX, Idea.guessType("Property",null,1.0,0.5)));
                                        message.add(Idea.createIdea("Y",(int)jewelY, Idea.guessType("Property",null,1.0,0.5)));
                                        message.add(Idea.createIdea("SPEED",creatureBasicSpeed, Idea.guessType("Property",null,1.0,0.5)));
                                        activation=0.8;

				}else{//Stop
                                        message.add(Idea.createIdea("ACTION","GOTO", Idea.guessType("Property",null,1.0,0.5)));
                                        message.add(Idea.createIdea("X",(int)jewelX, Idea.guessType("Property",null,1.0,0.5)));
                                        message.add(Idea.createIdea("Y",(int)jewelY, Idea.guessType("Property",null,1.0,0.5)));
                                        message.add(Idea.createIdea("SPEED",0, Idea.guessType("Property",null,1.0,0.5)));
                                        activation=0.0;
				}
                                System.out.println("Closest Jewel");
                                System.out.println(message.toStringFull());
				legsMO.setI(toJson(message),activation,name);
			} catch (JSONException e) {
				e.printStackTrace();
			}	
		}
                else {
                    activation=0.0;
                    legsMO.setI("",activation,name);
                }
                
	}//end proc
        
        @Override
        public void calculateActivation() {
        
        }
        
        String toJson(Idea i) {
            String q = "\"";
            String out = "{";
            String val;
            int ii=0;
            for (Idea il : i.getL()) {
                if (il.getL().isEmpty()) {
                    if (il.isNumber()) val = il.getValue().toString();
                    else val = q+il.getValue()+q;
                }
                else val = toJson(il);
                if (ii == 0) out += q+il.getName()+q+":"+val;
                else out += ","+q+il.getName()+q+":"+val;
                ii++;
            }
            out += "}";
            return out;
        }

}
