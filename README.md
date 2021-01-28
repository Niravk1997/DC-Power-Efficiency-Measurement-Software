# DC Power Efficiency Measurement Software

 A fully automated software that measures power efficiency among other things for various input voltages and output load currents for your circuit using a programmable power supply and a dc electronic load. This software supports Window 10, 8, 8.1, and 7.

**Features:**

- Fully automated, the user only need to set the input parameters and click start, the software will do the rest.
- Automatically save all the measured and calculated data to text files.
- Graphs and the measurement table are automatically updated as the software finishes each test.
- Graphs are highly interactive and customizable. Add vertical and or horizontal markers as well as set color themes. Use mouse tracker to easily see x and y values on the graph.
- Save graph as an image, and save table's content manually as .CSV or text file. You may also edit the table's content or delete them.
- Supports most HP/Agilent/Keysight power supplies that have a serial port.
- Supports SCPI commands by default for controlling power supplies.
- Software has safety checks preventing users from setting invalid parameters that may result in instruments showing errors.
- Every test is saved in its own custom folder with date time stamp.

**Supported Power Supplies**: Any programmable power supply with a serial interface that allows you to set voltage, set current, measure voltage, and     measure current should work with this software. Output on and output off commands are optional. Your power supply must operate in constant voltage mode (CV) while performing tests.

###### Currently tested power supplies: HP/Agilent/Keysight 66312A, 66332A, 6631B, 6632B, 6633B, 6634B, 6611C, 6612C, 6613C, and 6614C

**Supported DC Electronic Loads**: Any programmable dc electronic load or 2/4 quadrant power supply (must have programmable sink capability) with a serial interface that allows you to set the sink current value, measure voltage, and measure current should work with this software. Output on, output off, set voltage commands are optional. Your dc electronic load must operate in constant current sink mode (CC) while performing tests.

###### Currently tested dc electronic loads or 2/4 quadrant power supplies: HP/Agilent/Keysight 6631B, 6632B, 6633B, 6634B

![](Pictures\Test_Fixture.PNG)

The basic concept is that you connect your power supply to the input terminals of your device that you want to test. And connect your dc electronic load to the output terminals of your device. The power supply will provide power to your device and the dc electronic load will consume power from your device. The power supply will measure voltage and current on the input side of your device while the dc electronic load will measure voltage and current on the output side. **Please refer to the user manual for more info.**

![](Pictures\Picture_1.PNG)

![](Pictures\Picture_3.PNG)

![](Pictures\Picture_2.PNG)

![](Pictures\COM Port Window.PNG)

![](Pictures\Picture_4.PNG)

![](Pictures\Picture_5.PNG)

![](Pictures\Picture_6.PNG)

![](Pictures\Picture_7.PNG)

![](Pictures\Picture_8.PNG)

![](Pictures\Picture_9.PNG)

![](Pictures\Picture_10.PNG)

![](Pictures\Picture_11.PNG)

