   M S B A M L   `   `   `  ����L  HTektaRevitPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:TektaRevitPluginsTektaRevitPlugins  5   	   '    !TektaRevitPlugins.RebarsMarkerWnd   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:TektaRevitPlugins     5   	     � Title$  Rebar Marker��5   	   $	��350��6   $	��300��6+    � 
ResizeMode$ NoResize=�67    � 	Resources 5	       �� 
TargetType �� Property �� Value   	smallBtns��%�  '��       &	  �     '��     �� 5
   
   " ��6   .�5      ��  � ��$ 65��6&   �� 5       � ��$ 3q�6(   �� 5       � ��$ 3q�6'   5      �� 5   
   " ��6"   .�5      ��  � ��$ 30��6&   �� 5       � ��$ 3q�6(   �� 5       � ��$ 3q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 3q�6'   �� 5       �@��SelectionMode$ Multiple=�6.   5      5      .��5      � $��5q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5       $��* �6   �� 5!      $
��Auto �6   .+�5$   
   �� $��0��6   $��	Partition��6    
���   �.��5%      �� -   5         ��Name$ cb_partitions��5%       �� SelectedIndex$ 0��6.   $��3q�6@   5&      5'      �� 5)   
   -   5        	 ��Name$	 gb_host_mark��5)      $��1��6)   $��	Host Mark��66   
���   �.��5+      �� -   5        $ cb_host_marks��5+      $ 0��6.   $��3q�6@   5,      5-      �� 57   
   $��2��6   $��Assembly��6    
���   �.��59      � $��3q�6   
 � ColumnDefinitions
 5:      �� 5;      $��* �6'   �� 5<      $
��Auto �6'   �� 5=      $��* �6'   .+�5@      �� -   5         ��Name$ lb_available��5@      $��0��60   $��0��6=   $��75��6M   �� 5A      $��0��6   $��1��6*   $��Center=�6:   .+�5B      �� -   5         ��Name$ btn_in��5B      #��[  6-   $��In��6R   6_   �� 5C      -   5        $ btn_out��5C      #��[  6.   $	��Out��6Q   6_   5D      �� 5E      -   5        $ lb_selected��5E      $��0��6/   $��2��6<   5F      5G      �� 5I   
   $��4��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   .+�5J      �� -   5        $ btn_ok��5J      $��Ok��6%   62   �� 5K      -	   5        $ 
btn_cancel��5K      $��Cancel��6)   6:   5L      5M      5N      