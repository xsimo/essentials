package ca.xsimo.formationcontinue;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Set;

import com.google.gson.JsonArray;
import com.google.gson.JsonObject;

public class GsonWithTraditionalJava {
	
	static long perf;

	public static void main(JsonObject board) throws IOException {
		
		long start = System.currentTimeMillis();
		
		JsonArray lists = board.get("lists").getAsJsonArray();
		JsonArray cards = board.get("cards").getAsJsonArray();
		HashMap<String, String> listMap = new HashMap<String,String>();
		for(int j = 0; j < lists.size(); j++) {
			JsonObject list = lists.get(j).getAsJsonObject();
			listMap.put(list.get("id").getAsString(),list.get("name").getAsString());  
		}
		HashMap<String, List<String>> cardList = new HashMap<String, List<String>>();
		for(int i=0; i < cards.size();i++) {
			JsonObject card = cards.get(i).getAsJsonObject();
			String idList = card.get("idList").getAsString();
			if(!cardList.containsKey(idList)){
				cardList.put(idList, new ArrayList<String>());
			}
			cardList.get(idList).add(card.get("name").getAsString());
		}
		for(String s : cardList.keySet()) {
			Collections.sort(cardList.get(s));
		}
		for(String s : listMap.keySet()) {
			if(cardList.containsKey(s)) {
				System.out.println(listMap.get(s));
				List<String> subList = cardList.get(s);
				if(subList!=null) {
					for(int k=0;k < subList.size(); k++){
						System.out.println(subList.get(k));
					}
				}
				System.out.println();
			}else {
				System.out.println("no cards in list " + listMap.get(s));
			}
		}
		
		long end = System.currentTimeMillis();
		
		perf = end-start;
		System.out.println(GsonWithTraditionalJava.class.getName() + " program took " + perf + " milliseconds to execute");

	}

}
