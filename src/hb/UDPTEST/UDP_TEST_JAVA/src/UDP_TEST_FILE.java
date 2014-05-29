import java.net.*;
import java.io.*;

public class UDP_TEST_FILE {
	public static void main(String[] args)throws IOException{
		DatagramSocket ds = new DatagramSocket(20001);  //전송받을 '4444' 포트를 열어둔다.
		
		File path = new File("C:\\3DP");
		File img = new File(path, "01.png");
		
		int totalFileSize = 0;
		int nowSize = 0;
		
		FileOutputStream fos = new FileOutputStream(img);
		BufferedOutputStream bos = new BufferedOutputStream(fos);
		
		
		while(true){
			byte[] data = new byte[4096 * 10];
			// 한번에 받을 수 있는 최대 용량의 데이터 공간은 기볹정보 공간을 제외한 65,508이다.
			// 그래서 이 공간 만큼 미리 확보해둔다.
			DatagramPacket dp = new DatagramPacket(data, data.length);  //전송받을 객체생성
			
			ds.receive(dp);
			
			String tmp = new String(dp.getData()).trim();
			String header = tmp.substring(0, 4);
			
			if(header.equals("size") == true){
				totalFileSize = Integer.parseInt(tmp.substring(4, tmp.length()));
				System.out.println("fileSIze : " + totalFileSize);
			} else{
				System.out.println(nowSize);
				data = dp.getData();
				bos.write(data, 0, dp.getLength());
				nowSize += dp.getLength();
				System.out.println(nowSize);
				if(nowSize >= totalFileSize){
					bos.close();
					fos.close();
					ds.close();
					break;
				}
			}
		}
	}
}
