# blender_nodes.py — auto-create Principled BSDF material
import bpy
mat=bpy.data.materials.new('PBR'); mat.use_nodes=True
nodes,links=mat.node_tree.nodes,mat.node_tree.links
bsdf=nodes.get('Principled BSDF')
texC=nodes.new('ShaderNodeTexImage'); texC.image=bpy.data.images.load('//seamless_8k_pbr_3d_texture_of_twilight_sky_with_sun_halo_and_soft_cirrus_clouds_free_download__BaseColor.png'); links.new(texC.outputs['Color'], bsdf.inputs['Base Color'])
try:
    texN=nodes.new('ShaderNodeTexImage'); texN.image=bpy.data.images.load('//seamless_8k_pbr_3d_texture_of_twilight_sky_with_sun_halo_and_soft_cirrus_clouds_free_download__Normal_GL.png'); texN.image.colorspace_settings.name='Non-Color'
    nrm=nodes.new('ShaderNodeNormalMap'); links.new(texN.outputs['Color'], nrm.inputs['Color']); links.new(nrm.outputs['Normal'], bsdf.inputs['Normal'])
except: pass
for name,socket in [('Roughness','Roughness'),('Metallic','Metallic'),('AO','Base Color')]:
    try:
        tex=nodes.new('ShaderNodeTexImage'); tex.image=bpy.data.images.load('//seamless_8k_pbr_3d_texture_of_twilight_sky_with_sun_halo_and_soft_cirrus_clouds_free_download__'+name+'.png'); tex.image.colorspace_settings.name='Non-Color'
        if name=='AO':
            mix=nodes.new('ShaderNodeMixRGB'); mix.blend_type='MULTIPLY'; mix.inputs['Fac'].default_value=1.0
            links.new(tex.outputs['Color'], mix.inputs['Color2']); links.new(texC.outputs['Color'], mix.inputs['Color1']); links.new(mix.outputs['Color'], bsdf.inputs['Base Color'])
        else:
            links.new(tex.outputs['Color'], bsdf.inputs[socket])
    except: pass
try:
    texH=nodes.new('ShaderNodeTexImage'); texH.image=bpy.data.images.load('//seamless_8k_pbr_3d_texture_of_twilight_sky_with_sun_halo_and_soft_cirrus_clouds_free_download__Height.png'); texH.image.colorspace_settings.name='Non-Color'
    disp=nodes.new('ShaderNodeDisplacement'); links.new(texH.outputs['Color'], disp.inputs['Height']); links.new(disp.outputs['Displacement'], nodes['Material Output'].inputs['Displacement'])
except: pass
obj=bpy.context.active_object
if obj and obj.type=='MESH': obj.data.materials.append(mat)
