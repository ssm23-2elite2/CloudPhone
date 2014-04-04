
package com.ssm232.elite.cloudphonetest;

/////////////////////////////////////////////////////////////////////////////
//Sample driver class to demonstrate the use of CameraPreview class.
/////////////////////////////////////////////////////////////////////////////

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;

public class CloudPhone extends Activity implements View.OnClickListener {
    
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        this.setContentView(R.layout.layout_main);
        
        Button btn_start = (Button) findViewById(R.id.btn_start);
        btn_start.setOnClickListener(this);
        Button btn_stop = (Button) findViewById(R.id.btn_stop);
        btn_stop.setOnClickListener(this);
        Button btn_sample = (Button) findViewById(R.id.btn_sample);
        btn_sample.setOnClickListener(this);
    }

    @Override
    public void onClick(View v) {
        Intent intent;
        switch (v.getId()) {
            case R.id.btn_start:
                intent = new Intent("com.ssm232.elite.cloudphonetest");
                startService(intent);
                break;
            case R.id.btn_stop:
            	intent = new Intent("com.ssm232.elite.cloudphonetest");
            	stopService(intent);
                break;
            default:
                break;
        }
    }
}
