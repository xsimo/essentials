package ca.xsimo.formationcontinue;

import java.io.IOException;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;
import java.util.stream.Stream;
import java.util.stream.Stream.Builder;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;

public class GsonWithStreamApi {

	static long perf;
	
	public static void main(JsonObject board) throws IOException {
		
		long start = System.currentTimeMillis();
		
		JsonArray lists = board.get("lists").getAsJsonArray();
		JsonArray cards = board.get("cards").getAsJsonArray();
		Builder<JsonObject> columnStreamBuilder = Stream.builder();
		Builder<JsonObject> cardStreamBuilder = Stream.builder();
		lists.forEach( (JsonElement elt) -> { columnStreamBuilder.add(elt.getAsJsonObject()); });
		cards.forEach( (JsonElement elt) -> { cardStreamBuilder.add(elt.getAsJsonObject()); });
		Stream<JsonObject> columnStream = columnStreamBuilder.build();
		Stream<JsonObject> cardStream = cardStreamBuilder.build();
		Map<String, List<JsonObject>> cartesParTableaux = cardStream.collect(
			Collectors.groupingBy( (JsonObject card) -> card.get("idList").getAsString()) );
		columnStream.forEach(
			(JsonObject list) -> {
				List<JsonObject> cartesDeCetteListe = cartesParTableaux.get(list.get("id").getAsString());
				if(cartesDeCetteListe!=null && cartesDeCetteListe.size()>0) {
					Collections.sort(cartesDeCetteListe, new Comparator<JsonObject>() {
						@Override
						public int compare(JsonObject arg0, JsonObject arg1) {
							return arg0.get("name").getAsString().compareTo(arg1.get("name").getAsString());
						}
					});
					System.out.println(list.get("name").getAsString());
					cartesDeCetteListe.forEach(
						(JsonObject card) -> {
							System.out.println(card.get("name").getAsString());
						}
					);
					System.out.println();
				}
			}
		);
	
		long end = System.currentTimeMillis();
		
		perf = end-start;
		System.out.println(GsonWithStreamApi.class.getName() + " program took " + perf + " milliseconds to execute");

	}

}
