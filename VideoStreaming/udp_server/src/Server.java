import com.oracle.tools.packager.Log;

import java.io.File;
import java.io.FileInputStream;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.nio.file.Files;
import java.util.ArrayList;
import java.util.Arrays;

/**
 * Created by Avril on 2/4/17.
 */
public class Server  {

//    public static final String SERVERIP = "192.168.42.21";
    public static final String LOCAL_IP = "192.168.42.129";
    public static final int SERVER_PORT = 5554;
    public static final int LOCAL_PORT = 5554;

    public static final String FILE_NAME = "/Users/Avril/Desktop/udptest.png";

    public static void main( String args[] ){
        try {
            //server connecting
            InetAddress localAddr = InetAddress.getByName(LOCAL_IP);
            System.out.printf("S: Connecting... %s%n", localAddr);

            //server try initial receiving
            DatagramSocket socket = new DatagramSocket(SERVER_PORT);

            //
            byte[] buf = new byte[1024];
            DatagramPacket packet = new DatagramPacket(buf, buf.length);
            System.out.println("S: Receiving...");
            socket.receive(packet);
            System.out.println("S: Received: '" + (new String(packet.getData())).substring(0, packet.getLength()) + "'");

            int c = 5;
            while(c > 0){
                try (FileInputStream fis = new FileInputStream(FILE_NAME)) {

                    int read = fis.read(buf, 0, buf.length);
                    System.out.printf("Read: %d%n", read);
                    packet = new DatagramPacket(buf, read, localAddr, LOCAL_PORT);
                    System.out.println("S: Sending: pic");
                    socket.send(packet);
                    System.out.println("S: Sent: pic");
                    socket.receive(packet);
                    System.out.println("S: Received " + (new String(packet.getData())).substring(0, packet.getLength()));
                }
                c--;
            }

            //server done
            System.out.println("S: done");

        }catch (Exception e){
            e.printStackTrace();
        }
    }
}
