import java.net.*;
import java.io.*;

public class UDP_TEST_FILE {
	public static void main(String[] args)throws IOException{
		DatagramSocket ds = new DatagramSocket(20001);  //���۹��� '4444' ��Ʈ�� ����д�.
		
		File path = new File("C:\\3DP");
		File img = new File(path, "01.png");
		
		int totalFileSize = 0;
		int nowSize = 0;
		
		FileOutputStream fos = new FileOutputStream(img);
		BufferedOutputStream bos = new BufferedOutputStream(fos);
		
		
		while(true){
			byte[] data = new byte[4096 * 10];
			// �ѹ��� ���� �� �ִ� �ִ� �뷮�� ������ ������ ������� ������ ������ 65,508�̴�.
			// �׷��� �� ���� ��ŭ �̸� Ȯ���صд�.
			DatagramPacket dp = new DatagramPacket(data, data.length);  //���۹��� ��ü����
			
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
