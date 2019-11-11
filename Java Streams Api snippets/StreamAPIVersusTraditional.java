package ca.xsimo.formationcontinue;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.URLDecoder;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

public class StreamAPIVersusTraditional {

	public static void main(String[] args) throws IOException{
		
		//assuming first arguments is file that contains an exported Trello board in json
		String trelloFile = readRsrcFromClasspath(args[0]);
		com.google.gson.JsonParser jsonParser = new JsonParser();
		JsonObject board = jsonParser.parse(trelloFile).getAsJsonObject();
		
		GsonWithTraditionalJava.main(board.deepCopy());
		GsonWithStreamApi.main(board.deepCopy());
		
		long traditional = GsonWithTraditionalJava.perf;
		long withStreamAPI = GsonWithStreamApi.perf;
		
		System.out.println("traditional took " + traditional + " milliseconds and StreamAPI took " + withStreamAPI + " milliseconds ");
		
	}
	
	public static String readRsrcFromClasspath(String fileName) throws IOException{
		
		String theWorldOnAString = "";
		java.net.URL fileUrl = Thread.currentThread().getContextClassLoader().getResource(fileName);
		String filePath = URLDecoder.decode(fileUrl.getPath(), "UTF-8");
		BufferedReader reader = null;
		try{
			reader = new BufferedReader(new InputStreamReader(new FileInputStream(filePath),"UTF-8"));
			String line = null;
			while( (line = reader.readLine()) != null){
				theWorldOnAString += line;
			}
		}finally{
			try{
				reader.close();
			}catch(Exception e){
				//current exception is safe to ignore silently
			}
		}
		return theWorldOnAString;
	}

}
