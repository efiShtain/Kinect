   M S B A M L   `   `   `  ����G  CKinectServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:KinectServer.ConvertersKinectServer.Converters  5   	       KinectServer.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   <
converters%clr-namespace:KinectServer.Converters     5   	     � Title$  
MainWindow��5   	   $	��350��6   $	��525��6)    � 	Resources 5      2   ,KinectServer.Converters.KinectStateConverter   KinectStateConverter��%   &	          5   
   6*   5      .��5	      � -   5         �Name$	 grd��5	       � RowDefinitions 5
   
   �� 5      $��50 �6   �� 5      $��50 �6   �� 5      $��50 �6   �� 5      $��* �6   ��5   
   �� 5       �� ImageSource$ Images/background.jpg��6   .+�5      �� 
�������$��20(�6-   $��Center=�6;   .��6X   Kinect State6f   �� 5   
   $��1��6   
�������$��20(�65   $��Center=�6D   ��6a   �� *KinectState+ �� 	Converter# [  �� 5   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Center=�6;   .+�5   
   �� $��2��6   $��20(�6   
�������$��Center=�6E   .��6b   Player Name6o   �� 5   
   $��2��6   $��15(�6    $��Center=�6l   $	��250��6�   $��30��6�   ��6.   �� *Player+ �� UpdateSourceTrigger$ PropertyChanged=�6�   5      5      5      