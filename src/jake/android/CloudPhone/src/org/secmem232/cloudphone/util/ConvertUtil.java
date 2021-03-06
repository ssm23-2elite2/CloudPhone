package org.secmem232.cloudphone.util;

public class ConvertUtil {
	
	public static int getPositionalNumber(int src){
		int positionnalNumber=1;
		while(true){
			if(src/(int)Math.pow(10, positionnalNumber) == 0)
				break;
			positionnalNumber++;
		}
		return positionnalNumber;
	}
	
	public static int itoa(int src, byte[] buffer){
		int positionalNumber = getPositionalNumber(src);
		int length = positionalNumber;
				
		int i=0;
		while(positionalNumber > 0){
			int jesu = (int)Math.pow(10, positionalNumber-1);
			int quotiont = src / jesu;
			
			buffer[i] = (byte) (quotiont+'0');
			
			int remainder = src % jesu;		
			
			positionalNumber--;
			i++;
			src = remainder;
		}
		return length;
	}
	
	
	public static int itoa(int src, byte[] buffer, int positionalNumber, int offset){

		int i=offset;
		while(positionalNumber > 0){
			int jesu = (int)Math.pow(10, positionalNumber-1);
			int quotiont = src / jesu;

			buffer[i] = (byte) (quotiont+'0');

			int remainder = src % jesu;		

			positionalNumber--;
			i++;
			src = remainder;
		}
		return positionalNumber;
	}
	
	public static int itoa(int src, byte[] buffer, int offset){
		int positionalNumber = getPositionalNumber(src);
		int length = positionalNumber;
				
		int i=offset;
		while(positionalNumber > 0){
			int jesu = (int)Math.pow(10, positionalNumber-1);
			int quotiont = src / jesu;
			
			buffer[i] = (byte) (quotiont+'0');
			
			int remainder = src % jesu;		
			
			positionalNumber--;
			i++;
			src = remainder;
		}
		return length;
	}
}
