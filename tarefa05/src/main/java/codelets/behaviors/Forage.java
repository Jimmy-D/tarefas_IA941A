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

import br.unicamp.cst.core.entities.Codelet;
import br.unicamp.cst.core.entities.Memory;
import br.unicamp.cst.core.entities.MemoryContainer;
import br.unicamp.cst.core.entities.MemoryObject;
import java.util.List;
import org.json.JSONException;
import org.json.JSONObject;
import ws3dproxy.model.Thing;

/** 
 * 
 * @author klaus
 * 
 * 
 */

public class Forage extends Codelet {
    
        private Memory knownApplesMO;
        private Memory knownJewelsMO;
        private List<Thing> knownApples;
        private List<Thing> knownJewels;
        private MemoryContainer legsMO;


	/**
	 * Default constructor
	 */
	public Forage(){
            this.name = "Forage";
	}

	@Override
	public void proc() {
            knownApples = (List<Thing>) knownApplesMO.getI();
            knownJewels = (List<Thing>) knownJewelsMO.getI();
            if (knownApples.size() == 0 || knownJewels.size() == 0) {
		JSONObject message=new JSONObject();
			try {
				message.put("ACTION", "FORAGE");
                                activation=0.4;
				legsMO.setI(message.toString(),activation,name);
			} catch (JSONException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
            }
            else activation=0.0;
            JSONObject message=new JSONObject();
            message.put("ACTION", "FORAGE");
            legsMO.setI(message.toString(),activation,name);		
	}

	@Override
	public void accessMemoryObjects() {
            knownApplesMO = (MemoryObject)this.getInput("KNOWN_APPLES");
            knownJewelsMO = (MemoryObject)this.getInput("KNOWN_JEWELS");
            legsMO = (MemoryContainer)this.getOutput("LEGS");
	}
        
        @Override
        public void calculateActivation() {
            
        }


}
